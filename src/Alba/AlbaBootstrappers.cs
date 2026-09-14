using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Alba;

internal sealed class HostBuilderBootstrapper : IAlbaBootstrapper
{
    private readonly IHostBuilder _builder;

    public HostBuilderBootstrapper(IHostBuilder builder)
    {
        _builder = builder;
    }

    public async Task<IAlbaHost> StartAsync(IReadOnlyList<Action<IAlbaHostBuilder>> steps,
        IAlbaExtension[] extensions)
    {
        _builder.ConfigureServices(services =>
        {
            services.AddHttpContextAccessor();
            services.AddSingleton<IServer, TestServer>();
        });

        var adapter = new HostBuilderAdapter(_builder);
        foreach (var step in steps) step(adapter);

        var host = await _builder.StartAsync();

        AlbaHost albaHost;
        try
        {
            albaHost = new AlbaHost(host, extensions);
        }
        catch
        {
            await AlbaHost.StopQuietly(host);
            throw;
        }

        return await AlbaHost.StartExtensions(albaHost, extensions);
    }
}

internal sealed class WebApplicationBuilderBootstrapper : IAlbaBootstrapper
{
    private readonly WebApplicationBuilder _builder;
    private readonly Action<WebApplication> _configureRoutes;

    public WebApplicationBuilderBootstrapper(WebApplicationBuilder builder, Action<WebApplication> configureRoutes)
    {
        _builder = builder;
        _configureRoutes = configureRoutes;
    }

    public async Task<IAlbaHost> StartAsync(IReadOnlyList<Action<IAlbaHostBuilder>> steps,
        IAlbaExtension[] extensions)
    {
        _builder.Services.AddHttpContextAccessor();
        _builder.WebHost.UseTestServer();

        var adapter = new WebApplicationBuilderAdapter(_builder);
        foreach (var step in steps) step(adapter);

        var app = _builder.Build();
        _configureRoutes(app);

        await app.StartAsync();

        AlbaHost host;
        try
        {
            host = new AlbaHost(app, extensions);
        }
        catch
        {
            await AlbaHost.StopQuietly(app);
            throw;
        }

        return await AlbaHost.StartExtensions(host, extensions);
    }
}

internal sealed class WebApplicationFactoryBootstrapper<TEntryPoint> : IAlbaBootstrapper where TEntryPoint : class
{
    private readonly Action<IWebHostBuilder> _configuration;

    public WebApplicationFactoryBootstrapper(Action<IWebHostBuilder> configuration)
    {
        _configuration = configuration;
    }

    public async Task<IAlbaHost> StartAsync(IReadOnlyList<Action<IAlbaHostBuilder>> steps,
        IAlbaExtension[] extensions)
    {
        JasperFxEnvironmentAutoStartHost.Enable();
        var factory = new AlbaWebApplicationFactory<TEntryPoint>(_configuration, steps);

        AlbaHost host;
        try
        {
            // The factory builds and starts the application when its TestServer is resolved
            host = new AlbaHost(factory, extensions);
        }
        catch
        {
            await AlbaHost.DisposeQuietly(factory);
            throw;
        }

        return await AlbaHost.StartExtensions(host, extensions);
    }
}
