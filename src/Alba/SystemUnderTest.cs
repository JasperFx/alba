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
        private readonly IWebHost _host;
        public RequestDelegate Invoker { get; }

        public HttpContext CreateContext()
        {
            return new StubHttpContext(Features, Services);
        }

        public IFeatureCollection Features { get; }
        public IServiceProvider Services { get; }



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

        public static SystemUnderTest ForStartup<T>(Action<IHostingEnvironment> configure = null, string rootPath = null) where T : class
        {
            var environment = new HostingEnvironment {ContentRootPath = rootPath ?? FindParallelFolder(typeof(T).GetTypeInfo().Assembly.GetName().Name) ?? AppContext.BaseDirectory};

            configure?.Invoke(environment);

            var builder = new WebHostBuilder();
            builder.ConfigureServices(_ =>
            {
                _.AddSingleton<IHostingEnvironment>(environment);
                _.AddSingleton<IServer>(new TestServer());
            });

            builder.UseStartup<T>();

            var host = builder.Start();

            return new SystemUnderTest(host);
        }

        // How to get rid of the IServiceProvider?
        public static SystemUnderTest For(IServiceProvider services, Action<IApplicationBuilder> configuration)
        {
            var builder = new ApplicationBuilder(services);

            configuration(builder);

            return new SystemUnderTest(builder.Build(), builder.ServerFeatures, builder.ApplicationServices);
        }

        public SystemUnderTest(RequestDelegate invoker, IFeatureCollection features, IServiceProvider services)
        {
            Invoker = invoker;
            Features = features;
            Services = services;
        }

        private SystemUnderTest(IWebHost host)
        {
            Features = host.ServerFeatures;
            Services = host.Services;

            var field = typeof(WebHost).GetField("_application", BindingFlags.NonPublic | BindingFlags.Instance);
            Invoker = field.GetValue(host).As<RequestDelegate>();

            _host = host;
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
            _host?.Dispose();
        }
    }
}