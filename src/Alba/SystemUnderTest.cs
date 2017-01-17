using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Alba.Stubs;
using Baseline;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Internal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

namespace Alba
{
    public class SystemUnderTest : ISystemUnderTest
    {
        public RequestDelegate Invoker { get; }

        public HttpContext CreateContext()
        {
            return new StubHttpContext(Features, Services);
        }

        public IFeatureCollection Features { get; }
        public IServiceProvider Services { get; }

        public static SystemUnderTest ForStartup<T>() where T : class, new()
        {
            var builder = new WebHostBuilder();
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

            var field = typeof(WebHost).GetField("_application", BindingFlags.NonPublic);
            Invoker = field.GetValue(host).As<RequestDelegate>();
        }


        public virtual Task BeforeEach(HttpContext context)
        {
            return Task.CompletedTask;
        }

        public virtual Task AfterEach(HttpContext context)
        {
            return Task.CompletedTask;
        }

        public T FromJson<T>(string json)
        {
            throw new NotImplementedException();
        }

        public string ToJson(object target)
        {
            throw new NotImplementedException();
        }

        public string UrlFor<T>(Expression<Action<T>> expression, string method)
        {
            throw new NotImplementedException();
        }

        public string UrlFor<T>(string method)
        {
            throw new NotImplementedException();
        }

        public string UrlFor<T>(T input, string method)
        {
            throw new NotImplementedException();
        }
    }
}