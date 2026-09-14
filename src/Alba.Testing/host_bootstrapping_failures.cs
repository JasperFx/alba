using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;

namespace Alba.Testing;

public class host_bootstrapping_failures
{
    private readonly StopTracker theStopTracker = new();

    private IHostBuilder theBuilder()
    {
        return new HostBuilder().ConfigureWebHost(x =>
        {
            x.ConfigureServices(services => services.AddSingleton<IHostedService>(theStopTracker));
            x.Configure(app => app.Run(c => c.Response.WriteAsync("ok")));
        });
    }

    [Fact]
    public async Task a_failed_extension_start_tears_down_the_running_host()
    {
        var tracker = new TrackingExtension();

        var ex = await Should.ThrowAsync<DivideByZeroException>(
            async () => await AlbaHost.For(theBuilder(), tracker, new FailsToStartExtension()));

        ex.Message.ShouldBe("I cannot start");

        theStopTracker.WasStopped.ShouldBeTrue();
        tracker.WasAsyncDisposed.ShouldBeTrue();
    }

    [Fact]
    public async Task a_failed_extension_start_tears_down_a_web_application_factory_host()
    {
        var tracker = new TrackingExtension();

        await Should.ThrowAsync<DivideByZeroException>(
            async () => await AlbaHost.For<WebApp.Program>(tracker, new FailsToStartExtension()));

        tracker.WasAsyncDisposed.ShouldBeTrue();
    }

    [Fact]
    public async Task teardown_finishes_every_step_even_when_an_extension_fails_to_dispose()
    {
        var tracker = new TrackingExtension();

        var host = await AlbaHost.For(theBuilder(), new FailsToDisposeExtension(), tracker);

        var ex = await Should.ThrowAsync<AggregateException>(async () => await host.DisposeAsync());

        ex.InnerExceptions.Single().ShouldBeOfType<DivideByZeroException>();

        // The extension after the failure and the host itself are still torn down
        tracker.WasAsyncDisposed.ShouldBeTrue();
        theStopTracker.WasStopped.ShouldBeTrue();
    }

    internal class StopTracker : IHostedService
    {
        public bool WasStopped { get; private set; }

        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task StopAsync(CancellationToken cancellationToken)
        {
            WasStopped = true;
            return Task.CompletedTask;
        }
    }

    internal class TrackingExtension : IAlbaExtension
    {
        public bool WasAsyncDisposed { get; private set; }

        public void Dispose()
        {
        }

        public ValueTask DisposeAsync()
        {
            WasAsyncDisposed = true;
            return ValueTask.CompletedTask;
        }

        public Task Start(IAlbaHost host) => Task.CompletedTask;
    }

    internal class FailsToStartExtension : IAlbaExtension
    {
        public void Dispose()
        {
        }

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;

        public Task Start(IAlbaHost host) => throw new DivideByZeroException("I cannot start");
    }

    internal class FailsToDisposeExtension : IAlbaExtension
    {
        public void Dispose()
        {
        }

        public ValueTask DisposeAsync() => throw new DivideByZeroException("I cannot be disposed");

        public Task Start(IAlbaHost host) => Task.CompletedTask;
    }
}
