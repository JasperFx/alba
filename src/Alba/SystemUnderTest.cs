using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Alba.Stubs;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
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
        private Func<HttpContext, Task> _afterEach = c => Task.CompletedTask;


        private Func<HttpContext, Task> _beforeEach = c => Task.CompletedTask;

        public SystemUnderTest(IWebHostBuilder builder, Assembly applicationAssembly = null)
        {
            builder.ConfigureServices(_ => { _.AddSingleton<IHttpContextAccessor, HttpContextAccessor>(); });

            Server = new TestServer(builder);
#if NETCOREAPP3_0
            Server.AllowSynchronousIO = true;
#endif


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
        public IServiceProvider Services => Server.Host.Services;

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
        Task ISystemUnderTest.AfterEach(HttpContext context)
        {
            return _afterEach(context);
        }


        /// <summary>
        ///     The underlying IoC container for the application
        /// </summary>
        IServiceProvider ISystemUnderTest.Services => Server.Host.Services;

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
        public SystemUnderTest AfterEach(Action<HttpContext> afterEach)
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
        public SystemUnderTest AfterEachAsync(Func<HttpContext, Task> afterEach)
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