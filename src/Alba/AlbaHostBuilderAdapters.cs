using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Alba;

/// <summary>
/// Applies configuration to IHostBuilder-based bootstrapping. On .NET 10 this also
/// serves the host builder supplied by WebApplicationFactory
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
        // Host configuration is the only configuration that reaches a WebApplicationBuilder
        // application before its own startup code runs; see dotnet/aspnetcore#37680
        _builder.ConfigureHostConfiguration(configure);
    }
}

#if NET11_0_OR_GREATER
/// <summary>
/// Applies configuration to a WebApplicationBuilder application bootstrapped through
/// WebApplicationFactory. Configuration lands on the WebApplicationBuilder as soon as
/// it is created, before the application's own startup code runs
/// </summary>
internal sealed class WebApplicationFactoryHostBuilderAdapter : IAlbaHostBuilder
{
    private readonly IHostBuilder _builder;
    private readonly List<Action<IConfigurationBuilder>> _configuration = new();

    public WebApplicationFactoryHostBuilderAdapter(IHostBuilder builder)
    {
        _builder = builder;
    }

    public void ConfigureServices(Action<IServiceCollection> configure)
    {
        // Applied when the application builds, after its own registrations, so these win
        _builder.ConfigureServices((_, services) => configure(services));
    }

    public void ConfigureConfiguration(Action<IConfigurationBuilder> configure)
    {
        _configuration.Add(configure);
    }

    public void ApplyTo(IHostApplicationBuilder builder)
    {
        foreach (var configure in _configuration) configure(builder.Configuration);
    }
}
#endif

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
