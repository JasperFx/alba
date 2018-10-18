using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Alba.Stubs;
using Baseline;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;

namespace Alba
{
    public abstract class SystemUnderTestBase : ISystemUnderTest
    {
        private readonly Lazy<IWebHost> _host;
        private readonly Lazy<RequestDelegate> _invoker;

        protected SystemUnderTestBase(IHostingEnvironment environment = null)
        {
            _host = new Lazy<IWebHost>(buildHost);
            _invoker = new Lazy<RequestDelegate>(() =>
            {
                var host = _host.Value;


                var method = host.GetType().GetMethod("BuildApplication", BindingFlags.NonPublic | BindingFlags.Instance);

                return method.Invoke(host, new object[0]).As<RequestDelegate>();
            });

            Environment = environment ?? new HostingEnvironment();

            if (Environment.ContentRootPath.IsNotEmpty() && Environment.ContentRootFileProvider == null)
            {
                Environment.ContentRootFileProvider = new PhysicalFileProvider(Environment.ContentRootPath);
            }

            if (Environment.WebRootPath.IsNotEmpty() && Environment.WebRootFileProvider == null)
            {
                Environment.WebRootFileProvider = new PhysicalFileProvider(Environment.WebRootPath);
            }
        }

        protected abstract IWebHost buildHost();

        public IHostingEnvironment Environment { get; }

        public Task Invoke(HttpContext context)
        {
            return _invoker.Value(context);
        }

        public IFeatureCollection Features => _host.Value.ServerFeatures;

        private RequestDelegate _beforeEach = c => Task.CompletedTask;
        private RequestDelegate _afterEach = c => Task.CompletedTask;
        
        /// <summary>
        /// Override to take some kind of action just before an Http request
        /// is executed.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task ISystemUnderTest.BeforeEach(HttpContext context)
        {
            return _beforeEach(context);
        }

        public SystemUnderTestBase BeforeEach(RequestDelegate beforeEach)
        {
            _beforeEach = beforeEach;
            return this;
        }
        
        public SystemUnderTestBase BeforeEach(Action<HttpContext> action)
        {
            _beforeEach = c =>
            {
                action(c);
                return Task.CompletedTask;
            };
            
            return this;
        }

        /// <summary>
        /// Override to take some kind of action immediately after
        /// an Http request executes
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        Task ISystemUnderTest.AfterEach(HttpContext context)
        {
            return _afterEach(context);
        }

        public SystemUnderTestBase AfterEach(RequestDelegate afterEach)
        {
            _afterEach = afterEach;
            return this;
        }
        
        public SystemUnderTestBase AfterEach(Action<HttpContext> afterEach)
        {
            _afterEach = c =>
            {
                afterEach(c);
                return Task.CompletedTask;
            };
            
            return this;
        }

        public HttpContext CreateContext()
        {
            return new StubHttpContext(Features, Services);
        }

        /// <summary>
        /// The underlying IoC container for the application
        /// </summary>
        public IServiceProvider Services => _host.Value.Services;

        /// <summary>
        /// Force the SystemUnderTest to bootstrap itself
        /// </summary>
        protected void ensureBootstrapped()
        {
            if (!_host.IsValueCreated)
            {
                var host = _host.Value;
            }
        }

        protected void assertHostNotStarted()
        {
            if (_host.IsValueCreated) throw new InvalidOperationException("The WebHost has already been started");
        }

        public void Dispose()
        {
            if (_host.IsValueCreated)
            {
                _host.Value.Dispose();
            }
        }

        /// <summary>
        /// Url lookup strategy for this system
        /// </summary>
        public virtual IUrlLookup Urls { get; set; } = new NulloUrlLookup();

        /// <summary>
        /// Can be overridden to customize the Json serialization
        /// </summary>
        /// <param name="json"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual T FromJson<T>(string json)
        {
            ensureBootstrapped();

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
            ensureBootstrapped();

            var serializer = JsonSerializer.Create(JsonSerializerSettings);

            var writer = new StringWriter();
            var jsonWriter = new JsonTextWriter(writer);
            serializer.Serialize(jsonWriter, target);

            return writer.ToString();
        }

        /// <summary>
        /// Governs the Json serialization of the out of the box SystemUnderTest.
        /// </summary>
        public JsonSerializerSettings JsonSerializerSettings { get; set; } = new JsonSerializerSettings();


    }
}