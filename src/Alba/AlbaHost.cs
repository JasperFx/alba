using System.Diagnostics;
using System.Text.Json;
using Alba.Assertions;
using Alba.Internal;
using Alba.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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

    internal AlbaHost(IHost host, params IAlbaExtension[] extensions)
    {
        _host = host;
        Server = host.GetTestServer();

        Extensions = extensions;

        (MvcStrategy, MinimalApiStrategy, DefaultJson) = buildJsonStrategies();
    }

    internal AlbaHost(IAlbaWebApplicationFactory factory, params IAlbaExtension[] extensions)
    {
        _factory = factory;
        // This version of the test server will internally startup when initialized here
        Server = factory.Server;

        Extensions = extensions;

        (MvcStrategy, MinimalApiStrategy, DefaultJson) = buildJsonStrategies();
    }

    private (IJsonStrategy? Mvc, IJsonStrategy MinimalApi, IJsonStrategy Default) buildJsonStrategies()
    {
        var jsonInput = findInputFormatter(MimeType.Json.Value);
        var jsonOutput = findOutputFormatter(MimeType.Json.Value);

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
    public IServiceProvider Services => _host?.Services ?? _factory!.Services;

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

        HttpContext? context = null;
        try
        {
            // A setup failure surfaces from Invoke with its original stack trace,
            // and the application never sees the half configured request
            context = await Invoke(c =>
            {
                if (scenario.Claims.Count > 0)
                {
                    c.Items.Add(Alba.Scenario.ClaimsItemKey, scenario.Claims.ToArray());
                }

                if (scenario.RemovedClaims.Count > 0)
                {
                    c.Items.Add(Alba.Scenario.RemovedClaimsItemKey, scenario.RemovedClaims.ToArray());
                }

                foreach (var pair in scenario.Items) c.Items.Add(pair.Key, pair.Value);

                foreach (var apply in _beforeEachSync) apply(c);

                scenario.SetupHttpContext(c);

                if (c.Request.Path == null)
                {
                    throw new InvalidOperationException("This scenario has no defined url");
                }
            });

            scenario.RunAssertions(context);

            context.Response.Body.Position = 0;

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

        Activity? activity = null;
        var handler = Server.CreateHandler(c =>
        {
            if (scenario.Claims.Count > 0)
            {
                c.Items.Add(Alba.Scenario.ClaimsItemKey, scenario.Claims.ToArray());
            }

            if (scenario.RemovedClaims.Count > 0)
            {
                c.Items.Add(Alba.Scenario.RemovedClaimsItemKey, scenario.RemovedClaims.ToArray());
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

            foreach (var func in _afterEach) await func(null);

            throw;
        }

        var statusFailure = scenario.StatusCodeIgnored
            ? null
            : StatusCodeExpectation.Failure(scenario.ExpectedStatusCode, (int)response.StatusCode);
        if (statusFailure != null)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            await cleanupFailedStream(response, invoker, activity);

            var ex = new ScenarioAssertionException();
            ex.Add(statusFailure);
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
        List<Exception>? failures = null;

        foreach (var extension in Extensions)
        {
            await tryTeardown(() => extension.DisposeAsync());
        }

        await tryTeardown(async () =>
        {
            if (_host is not null)
            {
                await _host.StopAsync();
                _host.Dispose();
            }
        });

        await tryTeardown(() =>
        {
            Server.Dispose();
            return ValueTask.CompletedTask;
        });

        await tryTeardown(() => _factory?.DisposeAsync() ?? ValueTask.CompletedTask);

        if (failures is not null)
        {
            throw new AggregateException("One or more failures while tearing down the AlbaHost", failures);
        }

        // Every teardown step runs even if an earlier one fails
        async ValueTask tryTeardown(Func<ValueTask> step)
        {
            try
            {
                await step();
            }
            catch (Exception e)
            {
                (failures ??= new List<Exception>()).Add(e);
            }
        }
    }


    /// <summary>
    /// Configure an AlbaHost for the supplied IHostBuilder. The application starts
    /// when the returned builder is awaited
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="extensions"></param>
    /// <returns></returns>
    public static AlbaHostBuilder For(IHostBuilder builder, params IAlbaExtension[] extensions)
    {
        ArgumentNullException.ThrowIfNull(builder);
        return new AlbaHostBuilder(new HostBuilderBootstrapper(builder)).WithExtensions(extensions);
    }

    /// <summary>
    /// Configure an AlbaHost for a WebApplicationBuilder. The application is built
    /// and started when the returned builder is awaited
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configureRoutes">Configure the WebApplication for routing and/or middleware</param>
    /// <param name="extensions"></param>
    /// <returns></returns>
    public static AlbaHostBuilder For(WebApplicationBuilder builder, Action<WebApplication> configureRoutes,
        params IAlbaExtension[] extensions)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configureRoutes);
        return new AlbaHostBuilder(new WebApplicationBuilderBootstrapper(builder, configureRoutes))
            .WithExtensions(extensions);
    }

    /// <summary>
    /// Configure an AlbaHost for an application bootstrapped through WebApplicationFactory.
    /// The application starts when the returned builder is awaited
    /// </summary>
    /// <typeparam name="TEntryPoint">A type in the entry point assembly of the application. Typically the Startup or Program classes can be used.</typeparam>
    /// <param name="configuration"></param>
    /// <param name="extensions"></param>
    /// <returns></returns>
    public static AlbaHostBuilder For<TEntryPoint>(Action<IWebHostBuilder> configuration,
        params IAlbaExtension[] extensions) where TEntryPoint : class
    {
        ArgumentNullException.ThrowIfNull(configuration);
        return new AlbaHostBuilder(new WebApplicationFactoryBootstrapper<TEntryPoint>(configuration))
            .WithExtensions(extensions);
    }

    /// <summary>
    /// Configure an AlbaHost for an application bootstrapped through WebApplicationFactory
    /// with the application defaults. The application starts when the returned builder is awaited
    /// </summary>
    /// <typeparam name="TEntryPoint">A type in the entry point assembly of the application. Typically the Startup or Program classes can be used.</typeparam>
    /// <param name="extensions"></param>
    /// <returns></returns>
    public static AlbaHostBuilder For<TEntryPoint>(params IAlbaExtension[] extensions) where TEntryPoint : class
    {
        return For<TEntryPoint>(_ => { }, extensions);
    }

    internal static async Task<IAlbaHost> StartExtensions(AlbaHost host, IAlbaExtension[] extensions)
    {
        try
        {
            foreach (var extension in extensions) await extension.Start(host);
        }
        catch
        {
            // The application is already running at this point, so tear it down
            // rather than leaking a TestServer for the rest of the test run
            await DisposeQuietly(host);
            throw;
        }

        return host;
    }

    internal static async Task DisposeQuietly(IAsyncDisposable host)
    {
        try
        {
            await host.DisposeAsync();
        }
        catch (Exception)
        {
            // A teardown failure here would only mask the startup failure that matters
        }
    }

    internal static async Task StopQuietly(IHost host)
    {
        try
        {
            await host.StopAsync();
        }
        catch (Exception)
        {
            // A teardown failure here would only mask the startup failure that matters
        }
        finally
        {
            host.Dispose();
        }
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
