using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Text.Json;
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

    private readonly List<Func<Scenario, Task>> _beforeEachAsync = new();
    private readonly List<Action<HttpContext>> _beforeEachSync = new();

    private AlbaHost(IHost host, params IAlbaExtension[] extensions)
    {
        _host = host;
        Server = host.GetTestServer();

        Extensions = extensions;

        (MvcStrategy, MinimalApiStrategy, DefaultJson) = buildJsonStrategies();
    }

    private (IJsonStrategy? Mvc, IJsonStrategy MinimalApi, IJsonStrategy Default) buildJsonStrategies()
    {
        var jsonInput = findInputFormatter("application/json");
        var jsonOutput = findOutputFormatter("application/json");

        IJsonStrategy? mvc = null;
        if (jsonInput != null && jsonOutput != null)
        {
            mvc = new FormatterSerializer(this, jsonInput, jsonOutput);
        }

        var minimalApi = new SystemTextJsonSerializer(this);

        return (mvc, minimalApi, mvc ?? minimalApi);
    }

    internal IJsonStrategy? MvcStrategy { get; }
    internal IJsonStrategy MinimalApiStrategy { get; }
    internal IJsonStrategy DefaultJson { get; }

    internal JsonSerializerOptions StjJsonOptions => ((SystemTextJsonSerializer)MinimalApiStrategy).Options;

    public IReadOnlyList<IAlbaExtension> Extensions { get; }

    /// <summary>
    ///     The underlying TestServer for additional functionality
    /// </summary>
    public TestServer Server { get; }

    public Task StartAsync(CancellationToken cancellationToken = new())
    {
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken = new())
    {
        var factoryHost = _factory?.CreatedHost;
        if (factoryHost is not null)
            await factoryHost.StopAsync(cancellationToken);

        if (_host is not null)
            await _host.StopAsync(cancellationToken);
    }

    /// <summary>
    ///     The root IoC container of the running application
    /// </summary>
    public IServiceProvider Services => _host?.Services ?? _factory!.Services ?? Server.Services;

    public void Dispose()
    {
        // Single teardown implementation lives in DisposeAsync; blocking once
        // at teardown is the only intentional block left in Alba
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }
    
    public IAlbaHost BeforeEach(Action<HttpContext> beforeEach)
    {
        _beforeEachSync.Add(beforeEach);

        return this;
    }
    
    public IAlbaHost AfterEach(Action<HttpContext?> afterEach)
    {
        _afterEach.Add(c =>
        {
            afterEach(c);
            return Task.CompletedTask;
        });

        return this;
    }

    public IAlbaHost BeforeEachAsync(Func<Scenario, Task> beforeEach)
    {
        _beforeEachAsync.Add(beforeEach);

        return this;
    }
    
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

        foreach (var prepare in _beforeEachAsync) await prepare(scenario);

        foreach (var preparation in scenario.AsyncPreparations) await preparation();

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

                    foreach (var apply in _beforeEachSync) apply(c);

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

            if (context.Response.Body.CanSeek)
            {
                context.Response.Body.Position = 0;
            }

            return new ScenarioResult(this, context);
        }
        finally
        {
            foreach (var func in _afterEach) await func(context);
        }
    }


    public async Task<SseStreamResult> StreamServerSentEvents(Action<Scenario> configure,
        CancellationToken cancellationToken = default)
    {
        var scenario = new Scenario(this);

        configure(scenario);

        if (scenario.HasResponseAssertions)
        {
            throw new InvalidOperationException(
                "Response assertions are not supported with StreamServerSentEvents(). Assert on the streamed events instead.");
        }

        foreach (var prepare in _beforeEachAsync) await prepare(scenario);

        foreach (var preparation in scenario.AsyncPreparations) await preparation();

        scenario.Rewind();

        Activity? activity = null;
        var handler = Server.CreateHandler(c =>
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

                foreach (var apply in _beforeEachSync) apply(c);

                // The placeholder request message maps to "/"; clear the path so
                // the missing-url check below still applies
                c.Request.Path = PathString.Empty;

                scenario.SetupHttpContext(c);

                if (c.Request.Path == null)
                {
                    throw new InvalidOperationException("This scenario has no defined url");
                }

                activity = AlbaTracing.StartRequestActivity(c.Request);
            }
            catch (Exception e)
            {
                scenario.Exception = e;
            }
        });

        var invoker = new HttpMessageInvoker(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, new Uri(Server.BaseAddress, "/"));

        HttpResponseMessage response;
        try
        {
            // HttpMessageInvoker does not buffer response content, so this returns
            // as soon as the application flushes its response headers
            response = await invoker.SendAsync(request, cancellationToken);
        }
        catch
        {
            invoker.Dispose();
            activity?.Dispose();
            throw;
        }

        if (scenario.Exception != null)
        {
            await cleanupFailedStream(response, invoker, activity);
            ExceptionDispatchInfo.Throw(scenario.Exception);
        }

        var statusCode = (int)response.StatusCode;
        var statusFailure = !scenario.StatusCodeIgnored && (scenario.ExpectedStatusCode.HasValue
            ? statusCode != scenario.ExpectedStatusCode.Value
            : statusCode < 200 || statusCode >= 300);
        if (statusFailure)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            await cleanupFailedStream(response, invoker, activity);

            var ex = new ScenarioAssertionException();
            ex.Add(scenario.ExpectedStatusCode.HasValue
                ? $"Expected status code {scenario.ExpectedStatusCode}, but was {statusCode}"
                : $"Expected a status code between 200 and 299, but was {statusCode}");
            ex.AddBody(body);
            throw ex;
        }

        var contentType = response.Content.Headers.ContentType?.MediaType;
        if (contentType != MimeType.EventStream.Value)
        {
            await cleanupFailedStream(response, invoker, activity);
            throw new InvalidOperationException(
                $"The response content type is '{contentType ?? "unknown"}', not '{MimeType.EventStream.Value}'");
        }

        activity?.SetTag(AlbaTracing.HttpStatusCode, (int)response.StatusCode);

        var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return new SseStreamResult(this, response, stream, invoker, activity, _afterEach);
    }

    private async Task cleanupFailedStream(HttpResponseMessage response, HttpMessageInvoker invoker,
        Activity? activity)
    {
        response.Dispose();
        invoker.Dispose();
        activity?.Dispose();

        foreach (var func in _afterEach) await func(null);
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

        var adapter = new HostBuilderAdapter(builder);
        foreach (var extension in extensions) extension.Configure(adapter);

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

        var adapter = new WebApplicationBuilderAdapter(builder);
        foreach (var extension in extensions)
        {
            extension.Configure(adapter);
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
    public static async Task<IAlbaHost> For<TEntryPoint>(Action<IWebHostBuilder> configuration,
        params IAlbaExtension[] extensions) where TEntryPoint : class
    {
        JasperFxEnvironmentAutoStartHost.Enable();
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
        return For<TEntryPoint>(_ => { }, extensions);
    }

    private AlbaHost(IAlbaWebApplicationFactory factory, params IAlbaExtension[] extensions)
    {
        _factory = factory;
        // This version of the test server will internally startup when initialized here
        Server = factory.Server;

        Extensions = extensions;

        (MvcStrategy, MinimalApiStrategy, DefaultJson) = buildJsonStrategies();
    }


    private OutputFormatter? findOutputFormatter(string contentType)
    {
        var options = Services.GetRequiredService<IOptionsMonitor<MvcOptions>>();
        return options.Get("").OutputFormatters.OfType<OutputFormatter>()
            .Where(x => x.SupportedMediaTypes.Contains(contentType))
            .OrderBy(rankJsonFormatter)
            .FirstOrDefault();
    }

    private InputFormatter? findInputFormatter(string contentType)
    {
        var options = Services.GetRequiredService<IOptionsMonitor<MvcOptions>>();
        return options.Get("").InputFormatters.OfType<InputFormatter>()
            .Where(x => x.SupportedMediaTypes.Contains(contentType))
            .OrderBy(rankJsonFormatter)
            .FirstOrDefault();
    }

    // Third-party formatters such as OData's register ahead of the framework's JSON
    // formatters and advertise "application/json", but cannot run outside their own
    // pipeline (GH-116). Prefer the formatter the application actually uses for plain JSON.
    private static int rankJsonFormatter(object formatter)
    {
        if (formatter is SystemTextJsonInputFormatter or SystemTextJsonOutputFormatter) return 0;

        // Alba doesn't reference Microsoft.AspNetCore.Mvc.NewtonsoftJson, so match by name;
        // walk base types so subclasses rank the same
        for (var type = formatter.GetType(); type != null; type = type.BaseType)
        {
            if (type.FullName is "Microsoft.AspNetCore.Mvc.Formatters.NewtonsoftJsonInputFormatter"
                or "Microsoft.AspNetCore.Mvc.Formatters.NewtonsoftJsonOutputFormatter")
            {
                return 1;
            }
        }

        return 2;
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

            // Buffer the response so all subsequent reads are seekable,
            // memory-only operations
            if (!context.Response.Body.CanSeek)
            {
                var buffered = new MemoryStream();
                await context.Response.Body.CopyToAsync(buffered);
                buffered.Position = 0;
                context.Response.Body = buffered;
            }

            return context;
        }
        finally
        {
            activity?.Dispose();
        }
    }
}