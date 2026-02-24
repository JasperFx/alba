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

    public AlbaWebApplicationFactory(Action<IWebHostBuilder> configuration, IAlbaExtension[] extensions)
    {
        _configuration = configuration;
        _extensions = extensions;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        DisableIdentityLogger(builder);

        builder.ConfigureServices(services =>
        {
            services.AddHttpContextAccessor();
        });

        _configuration(builder);

        base.ConfigureWebHost(builder);
    }

    private static void DisableIdentityLogger(IWebHostBuilder builder)
    {
        const string key = "Logging:LogLevel:Microsoft.Identity.Web";
        if (builder.GetSetting(key) is null)
            builder.UseSetting(key, nameof(LogLevel.None));
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        foreach (var extension in _extensions)
        {
            extension.Configure(builder);
        }

        return base.CreateHost(builder);
    }
}