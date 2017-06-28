using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using Alba.Stubs;
using Baseline;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Newtonsoft.Json;

namespace Alba
{
    /// <summary>
    /// Root host of Alba to govern and configure the underlying ASP.Net Core application
    /// </summary>
    public class SystemUnderTest : SystemUnderTestBase
    {
        private readonly WebHostBuilder _builder;
        private readonly IList<Action<IServiceCollection>> _registrations = new List<Action<IServiceCollection>>();
        private readonly IList<Action<IWebHostBuilder>> _configurations = new List<Action<IWebHostBuilder>>();


        /// <summary>
        /// Create a SystemUnderTest using the designated "Startup" type
        /// to configure the ASP.Net Core system
        /// </summary>
        /// <param name="rootPath"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static SystemUnderTest ForStartup<T>(string rootPath = null) where T : class
        {
            var environment = new HostingEnvironment
            {
                ContentRootPath = rootPath ?? DirectoryFinder.FindParallelFolder(typeof(T).GetTypeInfo().Assembly.GetName().Name) ?? AppContext.BaseDirectory
            };

            environment.WebRootPath = environment.ContentRootPath;


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

        private SystemUnderTest(IHostingEnvironment environment = null) : base(environment)
        {
            _builder = new WebHostBuilder();
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
                Environment.ContentRootPath = DirectoryFinder.FindParallelFolder(typeof(T).GetTypeInfo().Assembly.GetName().Name) ?? Directory.GetCurrentDirectory();
                Environment.ContentRootFileProvider = new PhysicalFileProvider(Environment.ContentRootPath);
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



        protected override IWebHost buildHost()
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