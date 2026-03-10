using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Shouldly;
using WebApp;

namespace Alba.Testing.Acceptance;

public class host_stop_usage_repeated(ITestOutputHelper output)
{
    private const int Count = 10;

    [Theory]
    [InlineData(typeof(Runtime1))]
    [InlineData(typeof(Runtime2))]
    public async Task does_not_fail_on_host_disposal_for_minimal_api(Type type)
    {
        var failedIterations = await Repeat(Count,
            async () =>
            {
                await using var host = await AlbaHost.For<Program>(builder =>
                {
                    builder.ConfigureServices(services =>
                        services.AddSingleton(typeof(IHostedService), type)
                            .AddSingleton(output));
                });
            }
        );

        failedIterations.ShouldBe(0);
    }

    [Theory]
    [InlineData(typeof(Runtime1))]
    [InlineData(typeof(Runtime2))]
    public async Task does_not_fail_on_host_stop_for_minimal_api(Type type)
    {
        var failedIterations = await Repeat(Count,
            async () =>
            {
                await using var host = await AlbaHost.For<Program>(builder =>
                {
                    builder.ConfigureServices(services =>
                        services.AddSingleton(typeof(IHostedService), type)
                            .AddSingleton(output));
                });

                await host.StopAsync(TestContext.Current.CancellationToken);
            }
        );

        failedIterations.ShouldBe(0);
    }

    [Theory]
    [InlineData(typeof(Runtime1))]
    [InlineData(typeof(Runtime2))]
    public async Task does_not_fail_on_host_disposal_for_mvc_app(Type type)
    {
        var failedIterations = await Repeat(Count,
            async () =>
            {
                await using var host = await AlbaHost.For<Startup>(builder =>
                {
                    builder.ConfigureServices(services =>
                        services.AddSingleton(typeof(IHostedService), type)
                            .AddSingleton(output));
                });
            }
        );

        failedIterations.ShouldBe(0);
    }

    [Theory]
    [InlineData(typeof(Runtime1))]
    [InlineData(typeof(Runtime2))]
    public async Task does_not_fail_on_host_stop_for_mvc_app(Type type)
    {
        var failedIterations = await Repeat(Count,
            async () =>
            {
                await using var host = await AlbaHost.For<Startup>(builder =>
                {
                    builder.ConfigureServices(services =>
                        services.AddSingleton(typeof(IHostedService), type)
                            .AddSingleton(output));
                });

                await host.StopAsync(TestContext.Current.CancellationToken);
            }
        );

        failedIterations.ShouldBe(0);
    }

    private async Task<int> Repeat(int count, Func<Task> action)
    {
        var failedIterations = 0;
        for (var i = 0; i < count; i++)
        {
            output.WriteLine($"Iteration #{i}: started, Thread: {Environment.CurrentManagedThreadId}");
            try
            {
                await action();
            }
            catch (AggregateException ex) when (ex.InnerException is ObjectDisposedException or NullReferenceException)
            {
                failedIterations++;
            }

            output.WriteLine(
                $"Iteration #{i}: stopped, Thread: {Environment.CurrentManagedThreadId}{Environment.NewLine}");
        }

        return failedIterations;
    }

    private class Runtime1(ILoggerFactory loggers, ITestOutputHelper output) : IHostedService
    {
        private bool _hasStopped;
        private readonly ILogger<Runtime1> _logger = loggers.CreateLogger<Runtime1>();

        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_hasStopped)
                return;

            await Task.Delay(0, cancellationToken); // just to use async
            output.WriteLine($"Stopping {GetHashCode()}, Thread: {Environment.CurrentManagedThreadId}");
            _hasStopped = true;

            try
            {
                _logger.LogWarning("_endpoints.DrainAsync()");
            }
            catch (Exception ex) when (ex.InnerException is ObjectDisposedException or NullReferenceException)
            {
                output.WriteLine($"Failed with {ex.InnerException.GetType()} {GetHashCode()}, " +
                                 $"Thread: {Environment.CurrentManagedThreadId}");
                throw;
            }

            output.WriteLine($"Stopped {GetHashCode()}, Thread: {Environment.CurrentManagedThreadId}");
        }
    }

    private class Runtime2(ILoggerFactory loggers, ITestOutputHelper output) : IHostedService
    {
        private bool _hasStopped;
        private readonly SemaphoreSlim _lock = new(1);
        private readonly ILogger<Runtime2> _logger = loggers.CreateLogger<Runtime2>();

        public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_hasStopped) return;
            await _lock.WaitAsync(cancellationToken);
            try
            {
                if (_hasStopped) return;

                output.WriteLine($"Stopping {GetHashCode()}, Thread: {Environment.CurrentManagedThreadId}");
                _hasStopped = true;

                try
                {
                    _logger.LogWarning("_endpoints.DrainAsync()");
                }
                catch (Exception ex) when (ex.InnerException is ObjectDisposedException or NullReferenceException)
                {
                    output.WriteLine($"Failed {ex.InnerException.GetType()} {GetHashCode()}, " +
                                     $"Thread: {Environment.CurrentManagedThreadId}");
                    throw;
                }

                output.WriteLine($"Stopped {GetHashCode()}, Thread: {Environment.CurrentManagedThreadId}");
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}