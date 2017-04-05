using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Alba.Stubs;
using Baseline;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Alba
{
    /// <summary>
    /// Root host of Alba to govern and configure the underlying ASP.Net Core application
    /// </summary>
    public class SystemUnderTest : ISystemUnderTest
    {
        public IHostingEnvironment Environment { get; }
        private readonly Lazy<IWebHost> _host;
        private readonly Lazy<RequestDelegate> _invoker;
        private readonly WebHostBuilder _builder;
        private readonly IList<Action<IServiceCollection>> _registrations = new List<Action<IServiceCollection>>();
        private readonly IList<Action<IWebHostBuilder>> _configurations = new List<Action<IWebHostBuilder>>();

        public RequestDelegate Invoker => _invoker.Value;

        public HttpContext CreateContext()
        {
            return new StubHttpContext(Features, Services);
        }

        public IFeatureCollection Features => _host.Value.ServerFeatures;

        /// <summary>
        /// The underlying IoC container for the application
        /// </summary>
        public IServiceProvider Services => _host.Value.Services;



        private static string FindParallelFolder(string folderName)
        {
            var starting = AppContext.BaseDirectory.ToFullPath();
            while (starting.Contains(Path.DirectorySeparatorChar + "bin"))
            {
                starting = starting.ParentDirectory();
            }

            var candidate = starting.ParentDirectory().AppendPath(folderName);

            return Directory.Exists(candidate) ? candidate : null;
        }

        /// <summary>
        /// Create a SystemUnderTest using the designated "Startup" type
        /// to configure the ASP.Net Core system
        /// </summary>
        /// <param name="rootPath"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static SystemUnderTest ForStartup<T>(string rootPath = null) where T : class
        {
            var environment = new HostingEnvironment {ContentRootPath = rootPath ?? FindParallelFolder(typeof(T).GetTypeInfo().Assembly.GetName().Name) ?? AppContext.BaseDirectory};

            var system = new SystemUnderTest(environment);
            system.UseStartup<T>();

            return system;
        }

        public static SystemUnderTest For(Action<IWebHostBuilder> configuration)
        {
            var system = new SystemUnderTest(new HostingEnvironment());

            system.Configure(configuration);

            return system;
        }

        private SystemUnderTest(IHostingEnvironment environment = null)
        {
            Environment = environment ?? new HostingEnvironment();
            _host = new Lazy<IWebHost>(buildHost);
            _builder = new WebHostBuilder();

            _invoker = new Lazy<RequestDelegate>(() =>
            {
                var host = _host.Value;
                var field = typeof(WebHost).GetField("_application", BindingFlags.NonPublic | BindingFlags.Instance);
                return field.GetValue(host).As<RequestDelegate>();
            });
        }

        public SystemUnderTest() : this(new HostingEnvironment())
        {
        }

        /// <summary>
        /// Use the Startup type T to configure the ASP.Net Core application
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void UseStartup<T>() where T : class
        {
            if (Environment.ContentRootPath.IsEmpty())
            {
                Environment.ContentRootPath = FindParallelFolder(typeof(T).GetTypeInfo().Assembly.GetName().Name) ?? Directory.GetCurrentDirectory();
            }

            Configure(x => x.UseStartup<T>());
        }

        /// <summary>
        /// Add extra system configuration to the underlying ASP.Net Core application
        /// </summary>
        /// <param name="configure"></param>
        public void Configure(Action<IWebHostBuilder> configure)
        {
            assertHostNotStarted();
            _configurations.Add(configure);
        }

        /// <summary>
        /// Modify the IoC service registrations for the underlying ASP.Net Core application
        /// </summary>
        /// <param name="configure"></param>
        public void ConfigureServices(Action<IServiceCollection> configure)
        {
            assertHostNotStarted();
            _registrations.Add(configure);
        }

        private void assertHostNotStarted()
        {
            if (_host.IsValueCreated) throw new InvalidOperationException("The WebHost has already been started");
        }

        private IWebHost buildHost()
        {
            _builder.ConfigureServices(_ =>
            {
                _.AddSingleton(Environment);
                _.AddSingleton<IServer>(new TestServer());
                _.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            });

            foreach (var registration in _registrations)
            {
                _builder.ConfigureServices(registration);
            }

            foreach (var configuration in _configurations)
            {
                configuration(_builder);
            }

            var host = _builder.Start();

            var settings = host.Services.GetService<JsonSerializerSettings>();
            if (settings != null)
            {
                JsonSerializerSettings = settings;
            }

            

            return host;
        }

        /// <summary>
        /// Governs the Json serialization of the out of the box SystemUnderTest.
        /// </summary>
        public JsonSerializerSettings JsonSerializerSettings { get; set; } = new JsonSerializerSettings();

        /// <summary>
        /// Override to take some kind of action just before an Http request
        /// is executed.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual Task BeforeEach(HttpContext context)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Override to take some kind of action immediately after
        /// an Http request executes
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public virtual Task AfterEach(HttpContext context)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Can be overridden to customize the Json serialization
        /// </summary>
        /// <param name="json"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual T FromJson<T>(string json)
        {
            if (!_host.IsValueCreated)
            {
                var host = _host.Value;
            }

            var serializer = JsonSerializer.Create(JsonSerializerSettings);

            var reader = new JsonTextReader(new StringReader(json));
            return serializer.Deserialize<T>(reader);
        }

        /// <summary>
        /// Can be overridden to customize the Json serialization
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public virtual string ToJson(object target)
        {
            if (!_host.IsValueCreated)
            {
                var host = _host.Value;
            }

            var serializer = JsonSerializer.Create(JsonSerializerSettings);

            var writer = new StringWriter();
            var jsonWriter = new JsonTextWriter(writer);
            serializer.Serialize(jsonWriter, target);

            return writer.ToString();
        }


        /// <summary>
        /// Url lookup strategy for this system
        /// </summary>
        public IUrlLookup Urls { get; set; } = new NulloUrlLookup();

        public void Dispose()
        {
            if (_host.IsValueCreated)
            {
                _host.Value.Dispose();
            }
        }
    }

    // SAMPLE: IUrlLookup
    public interface IUrlLookup
    {
        string UrlFor<T>(Expression<Action<T>> expression, string httpMethod);
        string UrlFor<T>(string method);
        string UrlFor<T>(T input, string httpMethod);
    }
    // ENDSAMPLE

    public class NulloUrlLookup : IUrlLookup
    {
        public virtual string UrlFor<T>(Expression<Action<T>> expression, string httpMethod)
        {
            throw new NotSupportedException("You will need to manually specify the Url");
        }

        public virtual string UrlFor<T>(string method)
        {
            throw new NotSupportedException("You will need to manually specify the Url");
        }

        public virtual string UrlFor<T>(T input, string httpMethod)
        {
            throw new NotSupportedException("You will need to manually specify the Url");
        }
    }
}