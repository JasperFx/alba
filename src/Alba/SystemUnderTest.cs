using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Alba.Stubs;
using Baseline;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Alba
{
    public class SystemUnderTest : ISystemUnderTest
    {
        public IHostingEnvironment Environment { get; }
        private readonly Lazy<IWebHost> _host;
        private readonly Action<IWebHostBuilder> _configuration;
        private readonly Lazy<RequestDelegate> _invoker;
        private readonly WebHostBuilder _builder;

        public RequestDelegate Invoker => _invoker.Value;

        public HttpContext CreateContext()
        {
            return new StubHttpContext(Features, Services);
        }

        public IFeatureCollection Features => _host.Value.ServerFeatures;
        public IServiceProvider Services => _host.Value.Services;



        public static string FindParallelFolder(string folderName)
        {
            var starting = AppContext.BaseDirectory.ToFullPath();
            while (starting.Contains(Path.DirectorySeparatorChar + "bin"))
            {
                starting = starting.ParentDirectory();
            }

            var candidate = starting.ParentDirectory().AppendPath(folderName);

            return Directory.Exists(candidate) ? candidate : null;
        }

        public static SystemUnderTest ForStartup<T>(string rootPath = null) where T : class
        {
            var environment = new HostingEnvironment {ContentRootPath = rootPath ?? FindParallelFolder(typeof(T).GetTypeInfo().Assembly.GetName().Name) ?? AppContext.BaseDirectory};

            return new SystemUnderTest(x => x.UseStartup<T>(), environment);
        }

        private SystemUnderTest(Action<IWebHostBuilder> configuration, IHostingEnvironment environment)
        {
            Environment = environment;
            _configuration = configuration;
            _host = new Lazy<IWebHost>(buildHost);
            _builder = new WebHostBuilder();

            _invoker = new Lazy<RequestDelegate>(() =>
            {
                var host = _host.Value;
                var field = typeof(WebHost).GetField("_application", BindingFlags.NonPublic | BindingFlags.Instance);
                return field.GetValue(host).As<RequestDelegate>();
            });
        }

        

        private IWebHost buildHost()
        {
            _builder.ConfigureServices(_ =>
            {
                _.AddSingleton(Environment);
                _.AddSingleton<IServer>(new TestServer());
                _.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            });

            _configuration(_builder);

            return _builder.Start();
        }


        public virtual Task BeforeEach(HttpContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task AfterEach(HttpContext context)
        {
            return Task.CompletedTask;
        }

        public virtual T FromJson<T>(string json)
        {
            var serializer = new JsonSerializer();

            var reader = new JsonTextReader(new StringReader(json));
            return serializer.Deserialize<T>(reader);
        }

        public virtual string ToJson(object target)
        {
            var serializer = new JsonSerializer();

            var writer = new StringWriter();
            var jsonWriter = new JsonTextWriter(writer);
            serializer.Serialize(jsonWriter, target);

            return writer.ToString();
        }

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

        public bool SupportsUrlLookup { get; protected set; } = false;

        public void Dispose()
        {
            if (_host.IsValueCreated)
            {
                _host.Value.Dispose();
            }
        }
    }
}