using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Alba;

/// <summary>
/// Applies extension configuration to IHostBuilder-based bootstrapping,
/// including the host builder supplied by WebApplicationFactory
/// </summary>
internal sealed class HostBuilderAdapter : IAlbaHostBuilder
{
    private readonly IHostBuilder _builder;

    public HostBuilderAdapter(IHostBuilder builder)
    {
        _builder = builder;
    }

    public void ConfigureServices(Action<IServiceCollection> configure)
    {
        _builder.ConfigureServices((_, services) => configure(services));
    }

    public void ConfigureConfiguration(Action<IConfigurationBuilder> configure)
    {
        // Host configuration is what reaches applications created through
        // WebApplicationFactory; see dotnet/aspnetcore#37680
        _builder.ConfigureHostConfiguration(configure);
    }
}

/// <summary>
/// Applies extension configuration directly to a WebApplicationBuilder
/// </summary>
internal sealed class WebApplicationBuilderAdapter : IAlbaHostBuilder
{
    private readonly WebApplicationBuilder _builder;

    public WebApplicationBuilderAdapter(WebApplicationBuilder builder)
    {
        _builder = builder;
    }

    public void ConfigureServices(Action<IServiceCollection> configure)
    {
        configure(_builder.Services);
    }

    public void ConfigureConfiguration(Action<IConfigurationBuilder> configure)
    {
        // Sources added to the ConfigurationManager here land after the
        // application's own sources and therefore take precedence
        configure(_builder.Configuration);
    }
}
