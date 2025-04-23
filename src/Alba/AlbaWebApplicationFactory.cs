using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Alba;

/// <inheritdoc cref="WebApplicationFactory{TEntryPoint}"/>
internal sealed class AlbaWebApplicationFactory<TEntryPoint> : WebApplicationFactory<TEntryPoint>, IAlbaWebApplicationFactory where TEntryPoint : class
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
        builder.ConfigureServices(services =>
        {
            services.AddHttpContextAccessor();
        });

        _configuration(builder);

        base.ConfigureWebHost(builder);
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