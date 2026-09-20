using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;

namespace Alba;

/// <inheritdoc cref="WebApplicationFactory{TEntryPoint}"/>
internal sealed class AlbaWebApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint>,
    IAlbaWebApplicationFactory where TEntryPoint : class
{
    private readonly Action<IWebHostBuilder> _configuration;
    private readonly IReadOnlyList<Action<IAlbaHostBuilder>> _steps;

#if NET11_0_OR_GREATER
    private WebApplicationFactoryHostBuilderAdapter? _adapter;
#endif

    public IHost? CreatedHost { get; private set; }

    public AlbaWebApplicationFactory(Action<IWebHostBuilder> configuration,
        IReadOnlyList<Action<IAlbaHostBuilder>> steps)
    {
        _configuration = configuration;
        _steps = steps;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services => { services.AddHttpContextAccessor(); });

        _configuration(builder);

        base.ConfigureWebHost(builder);
    }

#if NET11_0_OR_GREATER
    // Runs during the entry point's WebApplication.CreateBuilder call, before the
    // application's own startup code, so overridden configuration is readable there
    protected override void ConfigureWebApplicationBuilder(IHostApplicationBuilder hostApplicationBuilder)
    {
        _adapter?.ApplyTo(hostApplicationBuilder);
    }
#endif

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var adapter = new WebApplicationFactoryHostBuilderAdapter(builder);
#if NET11_0_OR_GREATER
        _adapter = adapter;
#endif
        // Extensions and caller configuration, in the order they were recorded
        foreach (var step in _steps) step(adapter);

        // Avoid using Windows EventLog as it can cause exceptions during host stop/disposal.
        builder.ConfigureLogging(DisableWindowsEventLoggerProvider);

        CreatedHost = base.CreateHost(builder);

        return CreatedHost;
    }

    private static void DisableWindowsEventLoggerProvider(ILoggingBuilder loggingBuilder)
    {
        loggingBuilder.Services
            .Where(sd =>
                sd.ServiceType == typeof(ILoggerProvider)
                && sd.ImplementationType == typeof(EventLogLoggerProvider))
            .ToList()
            .ForEach(sd => loggingBuilder.Services.Remove(sd));
    }
}
