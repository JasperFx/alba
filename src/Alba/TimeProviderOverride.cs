using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Time.Testing;

namespace Alba;

#region sample_TimeProviderOverride

/// <summary>
/// Alba extension that replaces the application's <see cref="TimeProvider"/> registration
/// with this controllable <see cref="FakeTimeProvider"/> for time-travel testing. Pass an
/// instance to AlbaHost.For(...), then drive time from the test with Advance(...) and
/// SetUtcNow(...). Time is frozen unless AutoAdvanceAmount is set.
/// </summary>
public sealed class TimeProviderOverride : FakeTimeProvider, IAlbaExtension
{
    /// <summary>
    /// Starts the clock at FakeTimeProvider's default of 2000-01-01T00:00:00Z
    /// </summary>
    public TimeProviderOverride()
    {
    }

    /// <summary>
    /// Starts the clock at the supplied instant
    /// </summary>
    public TimeProviderOverride(DateTimeOffset startDateTime) : base(startDateTime)
    {
    }

    void IAlbaExtension.Configure(IAlbaHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<TimeProvider>();
            services.AddSingleton<TimeProvider>(this);
        });
    }

    Task IAlbaExtension.Start(IAlbaHost host)
    {
        return Task.CompletedTask;
    }

    void IDisposable.Dispose()
    {
    }

    ValueTask IAsyncDisposable.DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}

#endregion
