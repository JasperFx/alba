using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Alba
{
    /// <summary>
    ///     Root host of Alba to govern and configure the underlying ASP.Net Core application
    /// </summary>
    public class SystemUnderTest : ISystemUnderTest
    {
        
        private Func<HttpContext?, Task> _afterEach = c => Task.CompletedTask;


        private Func<HttpContext, Task> _beforeEach = c => Task.CompletedTask;

        private readonly IHost _host;
        
        public SystemUnderTest(IHostBuilder builder, Assembly? applicationAssembly = null)
        {
            builder
                .ConfigureServices(_ =>
                {
                    _.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
                    _.AddSingleton<IServer>(x => new TestServer(x));
                });

            _host = builder.Start();
            Services = _host.Services;

            Server = _host.GetTestServer();

            Server.AllowSynchronousIO = true;


            var options = _host.Services.GetService<IOptions<MvcNewtonsoftJsonOptions>>()?.Value;
            var settings = options?.SerializerSettings;
            if (settings != null) JsonSerializerSettings = settings;

            var manager = _host.Services.GetService<ApplicationPartManager>();
            if (applicationAssembly != null) manager?.ApplicationParts.Add(new AssemblyPart(applicationAssembly));
        }
        
        [Obsolete("Pass in a IHostBuilder generic host instead. See: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/host/generic-host")]
        public SystemUnderTest(IWebHostBuilder builder, Assembly? applicationAssembly = null)
        {
            builder.ConfigureServices(_ => { _.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); });

            Server = new TestServer(builder);
            Services = Server.Host.Services;
            
            Server.AllowSynchronousIO = true;

            var settings = Server.Host.Services.GetService<JsonSerializerSettings>();
            if (settings != null) JsonSerializerSettings = settings;

            var manager = Server.Host.Services.GetService<ApplicationPartManager>();
            if (applicationAssembly != null) manager?.ApplicationParts.Add(new AssemblyPart(applicationAssembly));
        }

        /// <summary>
        /// The underlying TestServer for additional functionality
        /// </summary>
        public TestServer Server { get; }

        /// <summary>
        /// The root IoC container of the running application
        /// </summary>
        public IServiceProvider Services { get; }

        /// <summary>
        ///     Governs the Json serialization of the out of the box SystemUnderTest.
        /// </summary>
        public JsonSerializerSettings JsonSerializerSettings { get; set; } = new JsonSerializerSettings();

        /// <summary>
        /// Called immediately before a scenario is executed. Useful for setting up
        /// system state
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task ISystemUnderTest.BeforeEach(HttpContext context)
        {
            return _beforeEach(context);
        }

        /// <summary>
        /// Called immediately after the scenario is executed. Useful for cleaning up
        /// test state left behind
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task ISystemUnderTest.AfterEach(HttpContext? context)
        {
            return _afterEach(context);
        }


        /// <summary>
        ///     Can be overridden to customize the Json serialization
        /// </summary>
        /// <param name="json"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T ISystemUnderTest.FromJson<T>(string json)
        {
            var serializer = JsonSerializer.Create(JsonSerializerSettings);

            var reader = new JsonTextReader(new StringReader(json));
            return serializer.Deserialize<T>(reader);
        }

        /// <summary>
        ///     Can be overridden to customize the Json serialization
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        string ISystemUnderTest.ToJson(object target)
        {
            var serializer = JsonSerializer.Create(JsonSerializerSettings);

            var writer = new StringWriter();
            var jsonWriter = new JsonTextWriter(writer);
            serializer.Serialize(jsonWriter, target);

            return writer.ToString();
        }


        public void Dispose()
        {
            Server.Dispose();
            _host?.Dispose();
        }

        /// <summary>
        ///     Url lookup strategy for this system
        /// </summary>
        IUrlLookup ISystemUnderTest.Urls { get; set; } = new NulloUrlLookup();

        public Task<HttpContext> Invoke(Action<HttpContext> setup)
        {
            return Server.SendAsync(setup);
        }

        /// <summary>
        ///     Create a SystemUnderTest using the designated "Startup" type
        ///     to configure the ASP.NET Core system
        /// </summary>
        /// <param name="configure">Optional configuration of the IHostBuilder to be applied *after* the call to <c>IWebHostBuilder.UseStartup()</c></param>
        /// <param name="rootPath">Specify the content root directory to be used by the host.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>The system under test</returns>
        public static SystemUnderTest ForStartup<T>(Func<IHostBuilder, IHostBuilder>? configure = null,
            string? rootPath = null) where T : class
        {
            var builder = Host.CreateDefaultBuilder();
            builder.ConfigureWebHostDefaults(config => config.UseStartup<T>());

            if (configure != null) builder = configure(builder);

            builder.UseContentRoot(rootPath ?? DirectoryFinder.FindParallelFolder(typeof(T).Assembly.GetName().Name) ??
                                   AppContext.BaseDirectory);

            return new SystemUnderTest(builder, typeof(T).Assembly);
        }


        /// <summary>
        ///     Creates a SystemUnderTest from a default HostBuilder using the provided configuration.
        /// </summary>
        /// <param name="configuration">Optional configuration of the IHostBuilder to be applied *after* the call to UseStartup()</param>
        /// <returns>The system under test</returns>
        public static SystemUnderTest For(Action<IHostBuilder> configuration)
        {
            var builder = Host.CreateDefaultBuilder();

            configuration(builder);

            return new SystemUnderTest(builder);
        }

        /// <summary>
        ///     Creates a SystemUnderTest from a default HostBuilder using the provided <c>IWebHostBuilder</c>
        /// </summary>
        /// <param name="configuration">Optional configuration of the IWebHostBuilder to be applied *after* the call to UseStartup()</param>
        /// <returns>The system under test</returns>
        public static SystemUnderTest For(Action<IWebHostBuilder> configuration)
        {
            var builder = Host.CreateDefaultBuilder();
            
            builder.ConfigureWebHostDefaults(configuration);
            
            return new SystemUnderTest(builder);

        }


        /// <summary>
        /// Execute some kind of action before each scenario. This is NOT additive
        /// </summary>
        /// <param name="beforeEach"></param>
        /// <returns></returns>
        public SystemUnderTest BeforeEach(Action<HttpContext> beforeEach)
        {
            _beforeEach = c =>
            {
                beforeEach(c);
                return Task.CompletedTask;
            };
            
            return this;
        }


        /// <summary>
        /// Execute some clean up action immediately after executing each HTTP execution. This is NOT additive
        /// </summary>
        /// <param name="afterEach"></param>
        /// <returns></returns>
        public SystemUnderTest AfterEach(Action<HttpContext?> afterEach)
        {
            _afterEach = c =>
            {
                afterEach(c);
                return Task.CompletedTask;
            };

            return this;
        }
        
        /// <summary>
        /// Run some kind of set up action immediately before executing an HTTP request
        /// </summary>
        /// <param name="beforeEach"></param>
        /// <returns></returns>
        public SystemUnderTest BeforeEachAsync(Func<HttpContext, Task> beforeEach)
        {
            _beforeEach = beforeEach;
            
            return this;
        }

        /// <summary>
        /// Execute some clean up action immediately after executing each HTTP execution. This is NOT additive
        /// </summary>
        /// <param name="afterEach"></param>
        /// <returns></returns>
        public SystemUnderTest AfterEachAsync(Func<HttpContext?, Task> afterEach)
        {
            _afterEach = afterEach;

            return this;
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