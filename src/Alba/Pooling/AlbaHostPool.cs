using System.Runtime.CompilerServices;
using Alba.Internal;

namespace Alba.Pooling;

/// <summary>
/// Shares a limited number of running hosts between tests that run in parallel. Each test
/// rents a host, uses it alone, and disposes the lease to reset the host and return it.
/// Hosts start as tests need them, and each one is built by the factory for its slot, so
/// hosts can use separate databases or other infrastructure.
/// </summary>
public sealed class AlbaHostPool : IAsyncDisposable
{
    private readonly Func<AlbaHostSlot, CancellationToken, Task<IAlbaHost>> _factory;
    private readonly AlbaHostPoolOptions _options;

    private readonly object _lock = new();
    private readonly Stack<PooledHost> _idle = new();
    private readonly SortedSet<int> _freeSlots = new();
    private readonly LinkedList<TaskCompletionSource<Ticket>> _waiters = new();
    private readonly List<AlbaHostLease> _leases = new();

    // Resettable extensions of every host, so one instance can't be shared between hosts
    private readonly HashSet<IResettableAlbaExtension> _resettable = new(ReferenceEqualityComparer.Instance);

    private Exception? _startupFailure;
    private bool _disposed;

    /// <summary>
    /// Create a pool whose hosts are configured by the factory, which is called once for each
    /// host the pool starts
    /// </summary>
    /// <param name="factory"></param>
    /// <param name="options"></param>
    public AlbaHostPool(Func<AlbaHostSlot, AlbaHostBuilder> factory, AlbaHostPoolOptions? options = null)
        : this(validated(factory), options)
    {
    }

    /// <summary>
    /// Create a pool whose hosts are started by the factory, which is called once for each host
    /// the pool starts. Use this form to create per host resources before the host starts.
    /// </summary>
    /// <param name="factory">Returns a host started through AlbaHost.For(...)</param>
    /// <param name="options"></param>
    public AlbaHostPool(Func<AlbaHostSlot, CancellationToken, Task<IAlbaHost>> factory,
        AlbaHostPoolOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _factory = factory;
        _options = options ?? new AlbaHostPoolOptions();

        ArgumentOutOfRangeException.ThrowIfLessThan(_options.MaxHosts, 1, "options.MaxHosts");
        if (_options.RentTimeout <= TimeSpan.Zero && _options.RentTimeout != Timeout.InfiniteTimeSpan)
        {
            throw new ArgumentOutOfRangeException("options.RentTimeout", _options.RentTimeout,
                "The rent timeout must be positive or Timeout.InfiniteTimeSpan");
        }

        MaxHosts = _options.MaxHosts;
        for (var i = 0; i < MaxHosts; i++) _freeSlots.Add(i);
    }

    private static Func<AlbaHostSlot, CancellationToken, Task<IAlbaHost>> validated(
        Func<AlbaHostSlot, AlbaHostBuilder> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        return (slot, _) => factory(slot).StartAsync();
    }

    /// <summary>
    /// The most hosts the pool runs at once
    /// </summary>
    public int MaxHosts { get; }

    /// <summary>
    /// Rent a host for the calling test, waiting for one to be returned when every host is
    /// rented, or starting one when the pool has room. Dispose the lease, typically with
    /// 'await using', to return the host.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <param name="caller">Identifies the test in timeout messages; supplied by the compiler</param>
    /// <param name="callerFile">Supplied by the compiler</param>
    /// <param name="callerLine">Supplied by the compiler</param>
    /// <returns></returns>
    /// <exception cref="TimeoutException">No host was returned within the RentTimeout</exception>
    public async Task<AlbaHostLease> RentAsync(CancellationToken cancellationToken = default,
        [CallerMemberName] string caller = "", [CallerFilePath] string callerFile = "",
        [CallerLineNumber] int callerLine = 0)
    {
        var ticket = await takeTicket(cancellationToken);
        var pooled = ticket.Host ?? await create(ticket.Slot, cancellationToken);

        var lease = new AlbaHostLease(this, pooled, $"{caller} ({Path.GetFileName(callerFile)}:{callerLine})");
        lock (_lock) _leases.Add(lease);

        if (_options.OnRent is { } onRent)
        {
            try
            {
                await onRent(pooled.Host, pooled.Slot);
            }
            catch
            {
                lock (_lock) _leases.Remove(lease);
                await replaceQuietly(pooled);
                throw;
            }
        }

        return lease;
    }

    /// <summary>
    /// Start hosts in parallel ahead of the first tests, so startup failures surface here
    /// rather than in whichever test rents first
    /// </summary>
    /// <param name="hosts">How many hosts should be running, MaxHosts by default</param>
    /// <param name="cancellationToken"></param>
    public async Task WarmUpAsync(int? hosts = null, CancellationToken cancellationToken = default)
    {
        var target = Math.Min(hosts ?? MaxHosts, MaxHosts);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(target, nameof(hosts));

        var slots = new List<int>();
        lock (_lock)
        {
            throwIfUnavailable();

            // Hosts that are rented or still starting count as running
            var running = MaxHosts - _freeSlots.Count;
            while (running++ < target) slots.Add(takeFreeSlot());
        }

        await Task.WhenAll(slots.Select(async index =>
        {
            var pooled = await create(index, cancellationToken);
            await releaseOrDestroy(pooled);
        }));
    }

    private async Task<Ticket> takeTicket(CancellationToken cancellationToken)
    {
        LinkedListNode<TaskCompletionSource<Ticket>> waiter;
        lock (_lock)
        {
            throwIfUnavailable();

            if (_idle.TryPop(out var idle)) return new Ticket(idle, idle.Slot.Index);
            if (_freeSlots.Count > 0) return new Ticket(null, takeFreeSlot());

            waiter = _waiters.AddLast(new TaskCompletionSource<Ticket>(TaskCreationOptions.RunContinuationsAsynchronously));
        }

        using var timeout = new CancellationTokenSource(_options.RentTimeout);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);
        await using var registration = linked.Token.Register(() => abandon(waiter, cancellationToken));

        return await waiter.Value.Task;
    }

    // Tickets are only handed to waiters under the lock, so a waiter still in the list has none
    private void abandon(LinkedListNode<TaskCompletionSource<Ticket>> waiter, CancellationToken cancellationToken)
    {
        Exception failure;
        lock (_lock)
        {
            if (waiter.List is null) return;
            _waiters.Remove(waiter);

            failure = cancellationToken.IsCancellationRequested
                ? new OperationCanceledException(cancellationToken)
                : new TimeoutException(timeoutMessage());
        }

        waiter.Value.TrySetException(failure);
    }

    private string timeoutMessage()
    {
        var leases = _leases.Count == 0
            ? "None, the hosts are still starting."
            : string.Join("; ", _leases);

        return $"Timed out after {_options.RentTimeout} waiting for a host from the AlbaHostPool, because all {MaxHosts} hosts are in use. " +
               $"Leases not yet returned: {leases} A test that does not dispose its lease, for example with 'await using', keeps the host from the pool.";
    }

    private void throwIfUnavailable()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_startupFailure is not null) throw startupFailed();
    }

    private InvalidOperationException startupFailed()
    {
        return new InvalidOperationException(
            "A host in the AlbaHostPool failed to start, so the pool starts no more hosts. See the inner exception.",
            _startupFailure);
    }

    private async Task<PooledHost> create(int index, CancellationToken cancellationToken)
    {
        var slot = new AlbaHostSlot(index);
        IAlbaHost? started = null;
        PooledHost? pooled = null;
        try
        {
            started = await _factory(slot, cancellationToken);
            if (started is not AlbaHost host)
            {
                throw new InvalidOperationException(
                    "The AlbaHostPool factory must return a host started through AlbaHost.For(...)");
            }

            pooled = new PooledHost(host, slot);
            claimResettableExtensions(pooled);

            if (_options.OnCreated is { } onCreated) await onCreated(host, slot);

            pooled.Checkpoint();
            return pooled;
        }
        catch (Exception e)
        {
            await AlbaHost.DisposeQuietly(() => pooled is null ? disposeHost(started, slot) : destroy(pooled));

            var cancelled = e is OperationCanceledException && cancellationToken.IsCancellationRequested;
            lock (_lock)
            {
                if (_options.FailFastOnStartupFailure && !cancelled)
                {
                    _startupFailure ??= e;
                    _freeSlots.Add(index);
                    failWaiters(startupFailed);
                }
                else
                {
                    releaseSlot(index);
                }
            }

            throw;
        }
    }

    private void claimResettableExtensions(PooledHost pooled)
    {
        var resettable = pooled.Host.Extensions.OfType<IResettableAlbaExtension>().ToArray();

        lock (_lock)
        {
            var shared = resettable.FirstOrDefault(_resettable.Contains);
            if (shared is not null)
            {
                throw new InvalidOperationException(
                    $"This {shared.GetType().Name} is already used by another host in the AlbaHostPool. The pool resets it each time a host is returned, so create a new instance for each host inside the pool's factory.");
            }

            _resettable.UnionWith(resettable);
            pooled.Resettable = resettable;
        }
    }

    internal async Task ReturnAsync(AlbaHostLease lease)
    {
        var pooled = lease.Pooled;
        lock (_lock) _leases.Remove(lease);

        if (lease.Discarded)
        {
            try
            {
                await destroy(pooled);
            }
            finally
            {
                lock (_lock) releaseSlot(pooled.Slot.Index);
            }

            return;
        }

        try
        {
            await pooled.ResetAsync();

            if (_options.OnReturn is { } onReturn) await onReturn(pooled.Host, pooled.Slot);
        }
        catch
        {
            await replaceQuietly(pooled);
            throw;
        }

        await releaseOrDestroy(pooled);
    }

    private async Task replaceQuietly(PooledHost pooled)
    {
        await AlbaHost.DisposeQuietly(() => destroy(pooled));
        lock (_lock) releaseSlot(pooled.Slot.Index);
    }

    private async Task releaseOrDestroy(PooledHost pooled)
    {
        lock (_lock)
        {
            if (!_disposed)
            {
                if (nextWaiter() is { } waiter) waiter.TrySetResult(new Ticket(pooled, pooled.Slot.Index));
                else _idle.Push(pooled);

                return;
            }
        }

        await destroy(pooled);
    }

    // Called under the lock
    private int takeFreeSlot()
    {
        var index = _freeSlots.Min;
        _freeSlots.Remove(index);
        return index;
    }

    // Called under the lock
    private void releaseSlot(int index)
    {
        if (!_disposed && nextWaiter() is { } waiter) waiter.TrySetResult(new Ticket(null, index));
        else _freeSlots.Add(index);
    }

    // Called under the lock
    private TaskCompletionSource<Ticket>? nextWaiter()
    {
        var first = _waiters.First;
        if (first is null) return null;

        _waiters.RemoveFirst();
        return first.Value;
    }

    // Called under the lock
    private void failWaiters(Func<Exception> failure)
    {
        foreach (var waiter in _waiters) waiter.TrySetException(failure());
        _waiters.Clear();
    }

    private Task destroy(PooledHost pooled)
    {
        lock (_lock) _resettable.ExceptWith(pooled.Resettable);
        return disposeHost(pooled.Host, pooled.Slot);
    }

    private static Task disposeHost(IAlbaHost? host, AlbaHostSlot slot)
    {
        return Teardown.RunAllAsync($"One or more failures while disposing the host in {slot.Name}",
            [() => host?.DisposeAsync().AsTask() ?? Task.CompletedTask, slot.DisposeResourcesAsync]);
    }

    /// <summary>
    /// Dispose the idle hosts and fail any waiting rents. Hosts that are still rented are
    /// disposed when their leases are returned.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        PooledHost[] idle;
        lock (_lock)
        {
            if (_disposed) return;
            _disposed = true;

            idle = _idle.ToArray();
            _idle.Clear();

            failWaiters(() => new ObjectDisposedException(nameof(AlbaHostPool)));
        }

        var destroys = idle.Select(destroy).ToArray();
        try
        {
            await Task.WhenAll(destroys);
        }
        catch
        {
            throw new AggregateException("One or more failures while disposing the hosts in the AlbaHostPool",
                destroys.Where(x => x.IsFaulted).SelectMany(x => x.Exception!.InnerExceptions));
        }
    }

    // Either an idle host, or a free slot to start a host in
    private readonly record struct Ticket(PooledHost? Host, int Slot);

    internal sealed class PooledHost(AlbaHost host, AlbaHostSlot slot)
    {
        private AlbaHost.HookCheckpoint _hooks;

        public AlbaHost Host { get; } = host;

        public AlbaHostSlot Slot { get; } = slot;

        public IResettableAlbaExtension[] Resettable { get; set; } = [];

        // Each return resets the host to how it was once it started and OnCreated ran
        public void Checkpoint()
        {
            _hooks = Host.CheckpointHooks();
            foreach (var extension in Resettable) extension.Checkpoint();
        }

        public async Task ResetAsync()
        {
            Host.RestoreHooks(_hooks);
            foreach (var extension in Resettable) await extension.ResetAsync();
        }
    }
}
