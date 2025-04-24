using System.Diagnostics;
using System.Runtime.ExceptionServices;
using Alba.Internal;
using Alba.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Alba;

/// <summary>
///     Root host of Alba to govern and configure the underlying ASP.Net Core application
/// </summary>
public class AlbaHost : IAlbaHost
{
    private readonly IHost? _host;
    private readonly IAlbaWebApplicationFactory? _factory;

    private readonly List<Func<HttpContext?, Task>> _afterEach = new();


    private readonly List<Func<HttpContext, Task>> _beforeEach = new();

    private AlbaHost(IHost host, params IAlbaExtension[] extensions)
    {
        _host = host;
        Server = host.GetTestServer();

        Server.AllowSynchronousIO = true;

        Extensions = extensions;
            
        var jsonInput = findInputFormatter("application/json");
        var jsonOutput = findOutputFormatter("application/json");

        if (jsonInput != null && jsonOutput != null)
        {
            MvcStrategy = new FormatterSerializer(this, jsonInput, jsonOutput);
        }

        MinimalApiStrategy = new SystemTextJsonSerializer(this);

        DefaultJson = MvcStrategy ?? MinimalApiStrategy;

    }

    public AlbaHost(IHostBuilder builder, params IAlbaExtension[] extensions)
    {
        builder = builder
            .ConfigureServices(_ =>
            {
                _.AddHttpContextAccessor();
                _.AddSingleton<IServer, TestServer>();
            });

        foreach (var extension in extensions) builder = extension.Configure(builder);

        _host = builder.Start();

        Server = _host.GetTestServer();
        Server.AllowSynchronousIO = true;

        Extensions = extensions;

        foreach (var extension in extensions) extension.Start(this).GetAwaiter().GetResult();

        var jsonInput = findInputFormatter("application/json");
        var jsonOutput = findOutputFormatter("application/json");

        if (jsonInput != null && jsonOutput != null)
        {
            MvcStrategy = new FormatterSerializer(this, jsonInput, jsonOutput);
        }

        MinimalApiStrategy = new SystemTextJsonSerializer(this);

        DefaultJson = MvcStrategy ?? MinimalApiStrategy;
    }
        
    internal IJsonStrategy? MvcStrategy { get; }
    internal IJsonStrategy MinimalApiStrategy { get; }
    internal IJsonStrategy DefaultJson { get; }

    public IReadOnlyList<IAlbaExtension> Extensions { get; }

    /// <summary>
    ///     The underlying TestServer for additional functionality
    /// </summary>
    public TestServer Server { get; }

    public Task StartAsync(CancellationToken cancellationToken = new())
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = new())
    {
        return _host == null ? Task.CompletedTask : _host.StopAsync(cancellationToken);
    }

    /// <summary>
    ///     The root IoC container of the running application
    /// </summary>
    public IServiceProvider Services => _host?.Services ?? _factory!.Services ?? Server.Services;

    public void Dispose()
    {
        foreach (var extension in Extensions) extension.Dispose();
        Server.Dispose();
        _host?.StopAsync();
        _host?.Dispose();
        _factory?.Dispose();
    }


    /// <summary>
    ///     Execute some kind of action before each scenario. This is additive as of Alba v5
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
    ///     Execute some clean up action immediately after executing each HTTP execution. This is additive as of Alba v5
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
    ///     Run some kind of set up action immediately before executing an HTTP request. This is additive as of Alba v5
    /// </summary>
    /// <param name="beforeEach"></param>
    /// <returns></returns>
    public IAlbaHost BeforeEachAsync(Func<HttpContext, Task> beforeEach)
    {
        _beforeEach.Add(beforeEach);

        return this;
    }

    /// <summary>
    ///     Execute some clean up action immediately after executing each HTTP execution. This is additive as of Alba v5
    /// </summary>
    /// <param name="afterEach"></param>
    /// <returns></returns>
    public IAlbaHost AfterEachAsync(Func<HttpContext?, Task> afterEach)
    {
        _afterEach.Add(afterEach);

        return this;
    }

    #region sample_ScenarioSignature
    /// <summary>
    ///     Define and execute an integration test by running an Http request through
    ///     your ASP.Net Core system
    /// </summary>
    /// <param name="configure"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<IScenarioResult> Scenario(
            Action<Scenario> configure)
        #endregion

    {
        var scenario = new Scenario(this);
                
        configure(scenario);

        scenario.Rewind();
            
        HttpContext? context = null;
        try
        {
            context = await Invoke(c =>
            {
                    
                try
                {
                    if (scenario.Claims.Any())
                    {
                        c.Items.Add("alba_claims", scenario.Claims.ToArray());
                    }

                    if (scenario.RemovedClaims.Any())
                    {
                        c.Items.Add("alba_removed_claims", scenario.RemovedClaims.ToArray());
                    }

                    foreach (var pair in scenario.Items) c.Items.Add(pair.Key, pair.Value);

                    // No async available here :(
                    foreach (var func in _beforeEach) func(c).GetAwaiter().GetResult();

                    c.Request.Body.Position = 0;


                    scenario.SetupHttpContext(c);

                    if (c.Request.Path == null)
                    {
                        throw new InvalidOperationException("This scenario has no defined url");
                    }
                }
                catch (Exception e)
                {
                    scenario.Exception = e;
                }
            });
            if (scenario.Exception != null)
            {
                ExceptionDispatchInfo.Throw(scenario.Exception);
            }

            scenario.RunAssertions(context);
        }
        finally
        {
            foreach (var func in _afterEach) await func(context);
        }

        if (context.Response.Body.CanSeek)
        {
            context.Response.Body.Position = 0;
        }
            
        return new ScenarioResult(this, context);
    }


    public async ValueTask DisposeAsync()
    {
        foreach (var extension in Extensions) await extension.DisposeAsync();
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }
            
        Server.Dispose();

        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }
    }


    public static async Task<IAlbaHost> For(IHostBuilder builder, params IAlbaExtension[] extensions)
    {
        builder = builder
            .ConfigureServices(_ =>
            {
                _.AddHttpContextAccessor();
                _.AddSingleton<IServer, TestServer>();
            });

        foreach (var extension in extensions) builder = extension.Configure(builder);

        var host = await builder.StartAsync();

        var albaHost = new AlbaHost(host, extensions);

        foreach (var extension in extensions) await extension.Start(albaHost);

        return albaHost;
    }


    /// <summary>
    /// Create an AlbaHost using the new WebApplicationBuilder
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configureRoutes"></param>
    /// <param name="extensions"></param>
    /// <returns></returns>
    public static async Task<IAlbaHost> For(WebApplicationBuilder builder, Action<WebApplication> configureRoutes,
        params IAlbaExtension[] extensions)
    {
        builder.Services.AddHttpContextAccessor();
        builder.WebHost.UseTestServer();

        foreach (var extension in extensions)
        {
            extension.Configure(builder.Host);
        }

        var app = builder.Build();
        configureRoutes(app);

        await app.StartAsync();

        var host = new AlbaHost(app, extensions);

        foreach (var extension in extensions)
        {
            await extension.Start(host);
        }

        return host;
    }

       
                
    /// <summary>
    /// Creates an AlbaHost using an underlying WebApplicationFactory.
    /// </summary>
    /// <typeparam name="TEntryPoint">A type in the entry point assembly of the application. Typically the Startup or Program classes can be used.</typeparam>
    /// <param name="configuration"></param>
    /// <param name="extensions"></param>
    /// <returns></returns>
    public static async Task<IAlbaHost> For<TEntryPoint>(Action<IWebHostBuilder> configuration, params IAlbaExtension[] extensions) where TEntryPoint : class
    {
        var factory = new AlbaWebApplicationFactory<TEntryPoint>(configuration, extensions);

        var host = new AlbaHost(factory, extensions);

        foreach (var extension in extensions)
        {
            await extension.Start(host);
        }

        return host;
    }

    /// <summary>
    /// Creates an AlbaHost using an underlying WebApplicationFactory with the application defaults.
    /// </summary>
    /// <typeparam name="TEntryPoint">A type in the entry point assembly of the application. Typically the Startup or Program classes can be used.</typeparam>
    /// <param name="extensions"></param>
    /// <returns></returns>
    public static Task<IAlbaHost> For<TEntryPoint>(params IAlbaExtension[] extensions) where TEntryPoint : class
    {
        return For<TEntryPoint>(_ => {}, extensions);
    }

    private AlbaHost(IAlbaWebApplicationFactory factory, params IAlbaExtension[] extensions)
    {
        _factory = factory;
        // This version of the test server will internally startup when initialized here
        Server = factory.Server;

        Server.AllowSynchronousIO = true;

        Extensions = extensions;
            
        var jsonInput = findInputFormatter("application/json");
        var jsonOutput = findOutputFormatter("application/json");

        if (jsonInput != null && jsonOutput != null)
        {
            MvcStrategy = new FormatterSerializer(this, jsonInput, jsonOutput);
        }

        MinimalApiStrategy = new SystemTextJsonSerializer(this);

        DefaultJson = MvcStrategy ?? MinimalApiStrategy;
    }


    private OutputFormatter? findOutputFormatter(string contentType)
    {
        var options = Services.GetRequiredService<IOptionsMonitor<MvcOptions>>();
        return options.Get("").OutputFormatters.OfType<OutputFormatter>()
            .FirstOrDefault(x => x.SupportedMediaTypes.Contains(contentType));
    }

    private InputFormatter? findInputFormatter(string contentType)
    {
        var options = Services.GetRequiredService<IOptionsMonitor<MvcOptions>>();
        return options.Get("").InputFormatters.OfType<InputFormatter>()
            .FirstOrDefault(x => x.SupportedMediaTypes.Contains(contentType));
    }

    public async Task<HttpContext> Invoke(Action<HttpContext> setup)
    {
        Activity? activity = null;
        try
        {
            var context = await Server.SendAsync(c =>
            {
                setup(c);
                activity = AlbaTracing.StartRequestActivity(c.Request);
            });
            activity?.SetResponseTags(context.Response);
            return context;
        }
        catch (Exception e)
        {
            if (e.Message.Contains("The server has not been started or no web application was configured."))
            {
                await Server.Host.StartAsync();
                return await Server.SendAsync(setup);
            }

            throw;
        }
        finally
        {
            activity?.Dispose();
        }
    }


    /// <summary>
    ///     Creates a SystemUnderTest from a default HostBuilder using the provided <c>IWebHostBuilder</c>
    /// </summary>
    /// <param name="configuration">
    ///     Optional configuration of the IWebHostBuilder to be applied *after* the call to
    ///     UseStartup()
    /// </param>
    /// <returns>The system under test</returns>
    public static AlbaHost For(Action<IWebHostBuilder> configuration)
    {
        var builder = Host.CreateDefaultBuilder();

        builder.ConfigureWebHostDefaults(configuration);

        return new AlbaHost(builder);
    }
}
