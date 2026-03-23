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
    private readonly IAlbaExtension[] _extensions;

    public IHost? CreatedHost { get; private set; }

    public AlbaWebApplicationFactory(Action<IWebHostBuilder> configuration, IAlbaExtension[] extensions)
    {
        _configuration = configuration;
        _extensions = extensions;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services => { services.AddHttpContextAccessor(); });

        _configuration(builder);

        base.ConfigureWebHost(builder);
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        foreach (var extension in _extensions)
        {
            extension.Configure(builder);
        }

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