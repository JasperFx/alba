using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
        DisableIdentityLogger(builder);
        DisableWindowsEventLog(builder);

        builder.ConfigureServices(services => { services.AddHttpContextAccessor(); });

        _configuration(builder);

        base.ConfigureWebHost(builder);
    }

    private static void DisableIdentityLogger(IWebHostBuilder builder)
    {
        // Avoid using LogHelper.Logger static singleton from Microsoft.Identity.Web
        // as it captures ILogger from the "active" host potentially introducing side effects for other hosts.
        // See https://github.com/AzureAD/microsoft-identity-web/blob/50cbeb29b399dea8936e73cca6c846e3664d57c5/src/Microsoft.Identity.Web.TokenAcquisition/MicrosoftIdentityBaseAuthenticationBuilder.cs#L70
        builder.UseSetting("Logging:LogLevel:Microsoft.Identity.Web", nameof(LogLevel.None));
    }

    private static void DisableWindowsEventLog(IWebHostBuilder builder)
    {
        // Avoid using Windows EventLog as it can cause exceptions during host stop/disposal. 
        builder.UseSetting("Logging:EventLog:LogLevel:Default", nameof(LogLevel.None));
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        foreach (var extension in _extensions)
        {
            extension.Configure(builder);
        }

        CreatedHost = base.CreateHost(builder);

        return CreatedHost;
    }
}