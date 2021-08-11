using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
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
    public class AlbaHost : IAlbaHost
    {
        
        private List<Func<HttpContext?, Task>> _afterEach = new List<Func<HttpContext?, Task>>();


        private List<Func<HttpContext, Task>> _beforeEach = new List<Func<HttpContext, Task>>();

        private readonly IHost _host;

        public static async Task<IAlbaHost> For(IHostBuilder builder, params IAlbaExtension[] extensions)
        {
            builder = builder
                .ConfigureServices(_ =>
                {
                    _.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
                    _.AddSingleton<IServer>(x => new TestServer(x));
                });

            foreach (var extension in extensions)
            {
                builder = extension.Configure(builder);
            }

            var host = await builder.StartAsync();
            
            var albaHost = new AlbaHost(host, extensions);

            foreach (var extension in extensions)
            {
                await extension.Start(albaHost);
            }

            return albaHost;
        }

        private AlbaHost(IHost host, params IAlbaExtension[] extensions)
        {
            _host = host;
            Server = host.GetTestServer();
            
            Server.AllowSynchronousIO = true;

            Extensions = extensions;

            initializeJsonSerialization();
        }

        private void initializeJsonSerialization()
        {
            // TODO -- This will all need to change to be JSON serializer agnostic
            var options = _host.Services.GetService<IOptions<MvcNewtonsoftJsonOptions>>()?.Value;
            var settings = options?.SerializerSettings;
            if (settings != null) JsonSerializerSettings = settings;
        }


        public IReadOnlyList<IAlbaExtension> Extensions { get; }

        public AlbaHost(IHostBuilder builder, params IAlbaExtension[] extensions)
        {
            builder = builder
                .ConfigureServices(_ =>
                {
                    _.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
                    _.AddSingleton<IServer>(x => new TestServer(x));
                });

            foreach (var extension in extensions)
            {
                builder = extension.Configure(builder);
            }

            _host = builder.Start();

            Server = _host.GetTestServer();
            Server.AllowSynchronousIO = true;

            Extensions = extensions;

            initializeJsonSerialization();

            foreach (var extension in extensions)
            {
                extension.Start(this).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// The underlying TestServer for additional functionality
        /// </summary>
        public TestServer Server { get; }

        public Task StartAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return _host.StopAsync(cancellationToken);
        }

        /// <summary>
        /// The root IoC container of the running application
        /// </summary>
        public IServiceProvider Services => _host.Services;

        /// <summary>
        ///     Governs the Json serialization of the out of the box SystemUnderTest.
        /// </summary>
        public JsonSerializerSettings JsonSerializerSettings { get; set; } = new JsonSerializerSettings();


        /// <summary>
        ///     Can be overridden to customize the Json serialization
        /// </summary>
        /// <param name="json"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T IAlbaHost.FromJson<T>(string json)
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
        string IAlbaHost.ToJson(object target)
        {
            var serializer = JsonSerializer.Create(JsonSerializerSettings);

            var writer = new StringWriter();
            var jsonWriter = new JsonTextWriter(writer);
            serializer.Serialize(jsonWriter, target);

            return writer.ToString();
        }


        public void Dispose()
        {
            foreach (var extension in Extensions)
            {
                extension.Dispose();
            }
            Server.Dispose();
            _host?.Dispose();
        }

        /// <summary>
        ///     Url lookup strategy for this system
        /// </summary>
        IUrlLookup IAlbaHost.Urls { get; set; } = new NulloUrlLookup();

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
        public static AlbaHost ForStartup<T>(Func<IHostBuilder, IHostBuilder> configure = null,
            string? rootPath = null) where T : class
        {
            var builder = Host.CreateDefaultBuilder();
            builder.ConfigureWebHostDefaults(config => config.UseStartup<T>());

            if (configure != null) builder = configure(builder);

            builder.UseContentRoot(rootPath ?? DirectoryFinder.FindParallelFolder(typeof(T).Assembly.GetName().Name) ??
                                   AppContext.BaseDirectory);

            return new AlbaHost(builder);
        }
        
        


        /// <summary>
        ///     Creates a SystemUnderTest from a default HostBuilder using the provided <c>IWebHostBuilder</c>
        /// </summary>
        /// <param name="configuration">Optional configuration of the IWebHostBuilder to be applied *after* the call to UseStartup()</param>
        /// <returns>The system under test</returns>
        public static AlbaHost For(Action<IWebHostBuilder> configuration)
        {
            var builder = Host.CreateDefaultBuilder();
            
            builder.ConfigureWebHostDefaults(configuration);
            
            return new AlbaHost(builder);

        }


        /// <summary>
        /// Execute some kind of action before each scenario. This is additive as of Alba v5
        /// </summary>
        /// <param name="beforeEach"></param>
        /// <returns></returns>
        public IAlbaHost BeforeEach(Action<HttpContext> beforeEach)
        {
            _beforeEach.Add(c =>
            {
                beforeEach(c);
                return Task.CompletedTask;
            });
            
            return this;
        }


        /// <summary>
        /// Execute some clean up action immediately after executing each HTTP execution. This is additive as of Alba v5
        /// </summary>
        /// <param name="afterEach"></param>
        /// <returns></returns>
        public IAlbaHost AfterEach(Action<HttpContext?> afterEach)
        {
            _afterEach.Add(c =>
            {
                afterEach(c);
                return Task.CompletedTask;
            });

            return this;
        }
        
        /// <summary>
        /// Run some kind of set up action immediately before executing an HTTP request. This is additive as of Alba v5
        /// </summary>
        /// <param name="beforeEach"></param>
        /// <returns></returns>
        public IAlbaHost BeforeEachAsync(Func<HttpContext, Task> beforeEach)
        {
            _beforeEach.Add(beforeEach);
            
            return this;
        }

        /// <summary>
        /// Execute some clean up action immediately after executing each HTTP execution. This is additive as of Alba v5
        /// </summary>
        /// <param name="afterEach"></param>
        /// <returns></returns>
        public IAlbaHost AfterEachAsync(Func<HttpContext?, Task> afterEach)
        {
            _afterEach.Add(afterEach);

            return this;
        }
        
        // SAMPLE: ScenarioSignature
        /// <summary>
        ///     Define and execute an integration test by running an Http request through
        ///     your ASP.Net Core system
        /// </summary>
        /// <param name="system"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public async Task<IScenarioResult> Scenario(
                Action<Scenario> configure)
            // ENDSAMPLE
        {
            var scenario = new Scenario(this);


            configure(scenario);

            scenario.Rewind();

            HttpContext? context = null;
            try
            {
                context = await Invoke(async c =>
                {
                    // TODO -- Catch any exceptions, and throw somehow. Might have to smuggle them through the HttpContext

                    try
                    {
                        if (scenario.Claims.Any())
                        {
                            c.Items.Add("alba_claims", scenario.Claims.ToArray());
                        }
                    
                        // I know what you're thinking, this is stupid, you shouldn't 
                        // ever mix sync and async if you can help it, and yet tests
                        // for long running before each *wiI do NOT understand why this is soll* break if you try to 
                        // use async _beforeEach here. .
                        foreach (var func in _beforeEach)
                        {
                            func(c).GetAwaiter().GetResult();
                        }

                        c.Request.Body.Position = 0;


                        scenario.SetupHttpContext(c);

                        if (c.Request.Path == null) throw new InvalidOperationException("This scenario has no defined url");
                    }
                    catch (Exception e)
                    {
                        scenario.InvocationException = e;
                    }
                });
                
                scenario.RunAssertions(context);
            }
            finally
            {
                foreach (var func in _afterEach)
                {
                    await func(context);
                }
            }

            if (context.Response.Body.CanSeek)
            {
                context.Response.Body.Position = 0;
            }


            return new ScenarioResult(context, this);
        }


        public async ValueTask DisposeAsync()
        {
            foreach (var extension in Extensions)
            {
                await extension.DisposeAsync();
            }
            
            await _host.StopAsync();
            _host.Dispose();
            Server.Dispose();
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