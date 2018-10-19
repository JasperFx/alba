using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Alba.Stubs;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Alba
{
    /// <summary>
    ///     Root host of Alba to govern and configure the underlying ASP.Net Core application
    /// </summary>
    public class SystemUnderTest : ISystemUnderTest
    {
        private Action<HttpContext> _afterEach = c =>
        {
            
        };


        private Action<HttpContext> _beforeEach = c =>
        {
        };
        
        private readonly TestServer _server;

        public SystemUnderTest(IWebHostBuilder builder, Assembly applicationAssembly = null)
        {
            builder.ConfigureServices(_ => { _.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); });

            _server = new TestServer(builder);


            var settings = _server.Host.Services.GetService<JsonSerializerSettings>();
            if (settings != null) JsonSerializerSettings = settings;

            var manager = _server.Host.Services.GetService<ApplicationPartManager>();
            if (applicationAssembly != null) manager?.ApplicationParts.Add(new AssemblyPart(applicationAssembly));
        }

        /// <summary>
        ///     Governs the Json serialization of the out of the box SystemUnderTest.
        /// </summary>
        public JsonSerializerSettings JsonSerializerSettings { get; } = new JsonSerializerSettings();

        /// <summary>
        ///     Override to take some kind of action just before an Http request
        ///     is executed.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        void ISystemUnderTest.BeforeEach(HttpContext context)
        {
            _beforeEach(context);
        }

        /// <summary>
        ///     Override to take some kind of action immediately after
        ///     an Http request executes
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        void ISystemUnderTest.AfterEach(HttpContext context)
        {
            _afterEach(context);
        }


        /// <summary>
        ///     The underlying IoC container for the application
        /// </summary>
        IServiceProvider ISystemUnderTest.Services => _server.Host.Services;

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
            _server.Dispose();
        }

        /// <summary>
        ///     Url lookup strategy for this system
        /// </summary>
        IUrlLookup ISystemUnderTest.Urls { get; set; } = new NulloUrlLookup();

        public Task<HttpContext> Invoke(Action<HttpContext> setup)
        {
            return _server.SendAsync(setup);
        }

        /// <summary>
        ///     Create a SystemUnderTest using the designated "Startup" type
        ///     to configure the ASP.Net Core system
        /// </summary>
        /// <param name="configure">Optional configuration of the IWebHostBuilder to be applied *after* the call to UseStartup()</param>
        /// <param name="rootPath"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static SystemUnderTest ForStartup<T>(Func<IWebHostBuilder, IWebHostBuilder> configure = null,
            string rootPath = null) where T : class
        {
            var builder = WebHost.CreateDefaultBuilder();
            builder.UseStartup<T>();
            if (configure != null) builder = configure(builder);

            builder.UseContentRoot(rootPath ?? DirectoryFinder.FindParallelFolder(typeof(T).Assembly.GetName().Name) ??
                                   AppContext.BaseDirectory);

            var system = new SystemUnderTest(builder, typeof(T).Assembly);

            return system;
        }

        public static SystemUnderTest For(Action<IWebHostBuilder> configuration)
        {
            var builder = new WebHostBuilder();
            configuration(builder);

            var system = new SystemUnderTest(builder);


            return system;
        }

        public SystemUnderTest BeforeEach(Action<HttpContext> beforeEach)
        {
            _beforeEach = beforeEach;
            return this;
        }


        public SystemUnderTest AfterEach(Action<HttpContext> afterEach)
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