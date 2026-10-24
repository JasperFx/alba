using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;

namespace Alba.Pooling;

/// <summary>
/// A host rented from an AlbaHostPool. Use it like any other IAlbaHost, then dispose it to
/// reset the host and return it to the pool. BeforeEach and AfterEach actions added through
/// the lease only apply until it is disposed.
/// </summary>
public sealed class AlbaHostLease : IAlbaHost
{
    private readonly AlbaHostPool _pool;
    private readonly long _rentedAt = Environment.TickCount64;
    private int _returned;

    internal AlbaHostLease(AlbaHostPool pool, AlbaHostPool.PooledHost pooled, string rentedBy)
    {
        _pool = pool;
        Pooled = pooled;
        RentedBy = rentedBy;
    }

    internal AlbaHostPool.PooledHost Pooled { get; }

    internal string RentedBy { get; }

    internal bool Discarded { get; private set; }

    internal AlbaHost Inner => _returned == 0
        ? Pooled.Host
        : throw new ObjectDisposedException(nameof(AlbaHostLease),
            "This lease has been returned to the AlbaHostPool. Rent another host to keep using the pool.");

    /// <summary>
    /// The slot in the pool that the rented host occupies
    /// </summary>
    public AlbaHostSlot Slot => Pooled.Slot;

    /// <summary>
    /// Replace the host instead of returning it to the pool when the lease is disposed. Use
    /// this after a test changes something that resetting the host does not undo.
    /// </summary>
    public void Discard()
    {
        Discarded = true;
    }

    public TestServer Server => Inner.Server;

    public IServiceProvider Services => Inner.Services;

    public Task<IScenarioResult> Scenario(Action<Scenario> configure) => Inner.Scenario(configure);

    public Task<SseStreamResult> StreamServerSentEvents(Action<Scenario> configure,
        CancellationToken cancellationToken = default)
    {
        return Inner.StreamServerSentEvents(configure, cancellationToken);
    }

    // The pool removes these from the host when the lease is disposed
    public IAlbaHost BeforeEach(Action<HttpContext> beforeEach)
    {
        Inner.BeforeEach(beforeEach);
        return this;
    }

    public IAlbaHost AfterEach(Action<HttpContext?> afterEach)
    {
        Inner.AfterEach(afterEach);
        return this;
    }

    public IAlbaHost BeforeEachAsync(Func<Scenario, Task> beforeEach)
    {
        Inner.BeforeEachAsync(beforeEach);
        return this;
    }

    public IAlbaHost AfterEachAsync(Func<HttpContext?, Task> afterEach)
    {
        Inner.AfterEachAsync(afterEach);
        return this;
    }

    // The pool owns the host, which is already running
    public Task StartAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        throw new InvalidOperationException(
            "A pooled host keeps running until the AlbaHostPool is disposed. Dispose the lease to return the host, or call Discard() and then dispose the lease to replace it.");
    }

    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Reset the host and return it to the pool, or replace it after Discard()
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _returned, 1) == 1) return;
        await _pool.ReturnAsync(this);
    }

    public override string ToString()
    {
        var held = TimeSpan.FromMilliseconds(Environment.TickCount64 - _rentedAt);
        return $"{Slot.Name} rented by {RentedBy} for {held.TotalSeconds:0.0}s";
    }
}
