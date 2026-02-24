using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using WebApp;

namespace Alba.Testing.Acceptance;

public class host_stop_usage
{
    [Fact]
    public async Task stop_for_hosted_service_is_called_for_minimal_api()
    {
        await using var host = await AlbaHost.For<global::Program>(x =>
        {
            x.ConfigureServices(services =>
                services.AddHostedService<SimpleHostedService>()
            );
        });
        var hostedService = (SimpleHostedService)host.Services.GetRequiredService<IHostedService>();

        await host.StopAsync(TestContext.Current.CancellationToken);

        // As stop may be called twice for minimal api Take(2) is used.
        hostedService.Events.Take(2).ShouldBe(["Started", "Stopped"]);
    }

    [Fact]
    public async Task stop_for_hosted_service_is_called_on_host_disposal_for_minimal_api()
    {
        var host = await AlbaHost.For<global::Program>(x =>
        {
            x.ConfigureServices(services =>
                services.AddHostedService<SimpleHostedService>()
            );
        });
        var hostedService = (SimpleHostedService)host.Services.GetRequiredService<IHostedService>();

        await host.DisposeAsync();

        // As stop may be called twice for minimal api Take(2) is used.
        hostedService.Events.Take(2).ShouldBe(["Started", "Stopped"]);
    }

    [Fact]
    public async Task stop_for_hosted_service_is_called_on_host_stop_for_mvc_app()
    {
        await using var host = await AlbaHost.For<Startup>(x =>
        {
            x.ConfigureServices(services =>
                services.AddHostedService<SimpleHostedService>()
            );
        });
        var hostedService = (SimpleHostedService)host.Services.GetRequiredService<IHostedService>();

        await host.StopAsync(TestContext.Current.CancellationToken);

        hostedService.Events.ShouldBe(["Started", "Stopped"]);
    }

    [Fact]
    public async Task stop_for_hosted_service_is_called_on_host_disposal_for_mvc_app()
    {
        var host = await AlbaHost.For<Startup>(x =>
        {
            x.ConfigureServices(services =>
                services.AddHostedService<SimpleHostedService>()
            );
        });
        var hostedService = (SimpleHostedService)host.Services.GetRequiredService<IHostedService>();

        await host.DisposeAsync();

        hostedService.Events.ShouldBe(["Started", "Stopped"]);
    }

    public class SimpleHostedService : IHostedService
    {
        readonly List<string> _events = [];

        public IEnumerable<string> Events => _events;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _events.Add("Started");
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _events.Add("Stopped");
            return Task.CompletedTask;
        }
    }
}