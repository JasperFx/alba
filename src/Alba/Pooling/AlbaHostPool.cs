using System.Runtime.CompilerServices;
using Alba.Internal;

namespace Alba.Pooling;

/// <summary>
/// Shares a limited number of running hosts between tests that run in parallel. Each test
/// rents a host, uses it alone, and disposes the lease to reset the host and return it.
/// Hosts start as tests need them, and each one is built by the factory for its slot, so
/// hosts can use separate databases or other infrastructure. Use AlbaHostPool&lt;TState&gt; to
/// keep state of your own with each host.
/// </summary>
public sealed class AlbaHostPool : AlbaHostPool<AlbaHostSlot>
{
    /// <summary>
    /// Create a pool whose hosts are configured by the factory, which is called once for each
    /// host the pool starts
    /// </summary>
    /// <param name="factory"></param>
    /// <param name="options"></param>
    public AlbaHostPool(Func<AlbaHostSlot, AlbaHostBuilder> factory, AlbaHostPoolOptions? options = null)
        : base(withSlot(factory), options)
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
        : base(withSlot(factory), options)
    {
    }

    private static Func<AlbaHostSlot, CancellationToken, Task<(IAlbaHost, AlbaHostSlot)>> withSlot(
        Func<AlbaHostSlot, AlbaHostBuilder> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        return async (slot, _) => (await factory(slot), slot);
    }

    private static Func<AlbaHostSlot, CancellationToken, Task<(IAlbaHost, AlbaHostSlot)>> withSlot(
        Func<AlbaHostSlot, CancellationToken, Task<IAlbaHost>> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        return async (slot, cancellationToken) => (await factory(slot, cancellationToken), slot);
    }
}

/// <summary>
/// Shares a limited number of running hosts between tests that run in parallel, like
/// AlbaHostPool, and keeps state of your own with each host, such as its mocks or a
/// connection to its database. The state is disposed before its host when it implements
/// IAsyncDisposable or IDisposable.
/// </summary>
/// <typeparam name="TState"></typeparam>
public class AlbaHostPool<TState> : IAsyncDisposable
{
    private readonly Func<AlbaHostSlot, CancellationToken, Task<(IAlbaHost Host, TState State)>> _factory;
    private readonly AlbaHostPoolOptions<TState> _options;
    private readonly SemaphoreSlim? _starting;

    private readonly object _lock = new();
    private readonly Stack<PooledHost> _idle = new();
    private readonly SortedSet<int> _freeSlots = new();
    private readonly int[] _generations;
    private readonly LinkedList<TaskCompletionSource<Ticket>> _waiters = new();
    private readonly List<AlbaHostLease> _leases = new();

    // Resettable extensions of every host, so one instance can't be shared between hosts
    private readonly HashSet<IResettableAlbaExtension> _resettable = new(ReferenceEqualityComparer.Instance);

    private long _lastReturn;
    private Exception? _startupFailure;
    private bool _disposed;

    /// <summary>
    /// Create a pool whose factory starts each host and creates its state. The factory is
    /// called once for each host the pool starts, including hosts that replace discarded ones.
    /// </summary>
    /// <param name="factory">Returns a host started through AlbaHost.For(...) and its state</param>
    /// <param name="options"></param>
    public AlbaHostPool(Func<AlbaHostSlot, CancellationToken, Task<(IAlbaHost Host, TState State)>> factory,
        AlbaHostPoolOptions<TState>? options = null)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _factory = factory;
        _options = options ?? new AlbaHostPoolOptions<TState>();

        ArgumentOutOfRangeException.ThrowIfLessThan(_options.MaxHosts, 1, "options.MaxHosts");
        if (_options.RentTimeout <= TimeSpan.Zero && _options.RentTimeout != Timeout.InfiniteTimeSpan)
        {
            throw new ArgumentOutOfRangeException("options.RentTimeout", _options.RentTimeout,
                "The rent timeout must be positive or Timeout.InfiniteTimeSpan");
        }

        if (_options.MaxConcurrentStarts is { } maxStarts)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(maxStarts, 1, "options.MaxConcurrentStarts");
            _starting = new SemaphoreSlim(maxStarts);
        }

        MaxHosts = _options.MaxHosts;
        _generations = new int[MaxHosts];
        for (var i = 0; i < MaxHosts; i++) _freeSlots.Add(i);
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
    /// <param name="caller">Identifies the test in timeout messages. Supplied by the compiler, or pass the
    /// test's name when a base class or fixture rents for it.</param>
    /// <param name="callerFile">Supplied by the compiler</param>
    /// <param name="callerLine">Supplied by the compiler</param>
    /// <returns></returns>
    /// <exception cref="TimeoutException">No host was returned to the pool within the RentTimeout</exception>
    public async Task<AlbaHostLease<TState>> RentAsync(CancellationToken cancellationToken = default,
        [CallerMemberName] string caller = "", [CallerFilePath] string callerFile = "",
        [CallerLineNumber] int callerLine = 0)
    {
        using var activity = AlbaTracing.StartPoolActivity("rent");
        try
        {
            var waiting = Environment.TickCount64;
            var ticket = await takeTicket(cancellationToken);
            activity?.SetTag(AlbaTracing.PoolWaitMilliseconds, Environment.TickCount64 - waiting);
            activity?.SetTag(AlbaTracing.PoolSlot, ticket.Slot);

            var pooled = ticket.Host ?? await create(ticket.Slot, cancellationToken);

            var lease = new AlbaHostLease<TState>(pooled, $"{caller} ({Path.GetFileName(callerFile)}:{callerLine})",
                ReturnAsync);
            lock (_lock) _leases.Add(lease);

            if (_options.OnRent is { } onRent)
            {
                try
                {
                    await onRent(pooled.Host, stateOf(pooled));
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
        catch (Exception e)
        {
            AlbaTracing.RecordFailure(activity, e);
            throw;
        }
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

    private static TState stateOf(PooledHost pooled) => (TState)pooled.State!;

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

        await using var registration = cancellationToken.Register(() =>
            abandon(waiter, () => new OperationCanceledException(cancellationToken)));

        if (_options.RentTimeout != Timeout.InfiniteTimeSpan) await timeOutWhenStalled(waiter);

        return await waiter.Value.Task;
    }

    // Fails the waiter once no host has come back to the pool for the RentTimeout, however
    // long the queue of waiting tests is
    private async Task timeOutWhenStalled(LinkedListNode<TaskCompletionSource<Ticket>> waiter)
    {
        var waiting = Environment.TickCount64;
        var timeout = (long)_options.RentTimeout.TotalMilliseconds;

        while (true)
        {
            var quiet = Environment.TickCount64 - Math.Max(waiting, Volatile.Read(ref _lastReturn));
            if (quiet >= timeout)
            {
                abandon(waiter, () => new TimeoutException(timeoutMessage(waiting)));
                return;
            }

            using var delay = new CancellationTokenSource();
            var finished = await Task.WhenAny(waiter.Value.Task,
                Task.Delay(TimeSpan.FromMilliseconds(timeout - quiet), delay.Token));
            await delay.CancelAsync();

            if (finished == waiter.Value.Task) return;
        }
    }

    // Tickets are only handed to waiters under the lock, so a waiter still in the list has none
    private void abandon(LinkedListNode<TaskCompletionSource<Ticket>> waiter, Func<Exception> failure)
    {
        Exception exception;
        lock (_lock)
        {
            if (waiter.List is null) return;
            _waiters.Remove(waiter);
            exception = failure();
        }

        waiter.Value.TrySetException(exception);
    }

    // Called under the lock
    private string timeoutMessage(long waiting)
    {
        var waited = TimeSpan.FromMilliseconds(Environment.TickCount64 - waiting);
        var leases = _leases.Count == 0
            ? "None, the hosts are still starting."
            : string.Join("; ", _leases);

        return $"No host was returned to the AlbaHostPool for {_options.RentTimeout} while this rent waited {waited.TotalSeconds:0.0}s, " +
               $"and all {MaxHosts} hosts are in use. Leases not yet returned: {leases} " +
               "A test that does not dispose its lease, for example with 'await using', keeps the host from the pool.";
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

    // The slot's index belongs to this call until the host is released, so nothing else
    // touches its generation meanwhile
    private async Task<PooledHost> create(int index, CancellationToken cancellationToken)
    {
        var slot = new AlbaHostSlot(index, _generations[index]++);

        using var activity = AlbaTracing.StartPoolActivity("start host");
        activity?.SetTag(AlbaTracing.PoolSlot, index);
        activity?.SetTag(AlbaTracing.PoolSlotGeneration, slot.Generation);

        try
        {
            return await start(slot, cancellationToken);
        }
        catch (Exception e)
        {
            AlbaTracing.RecordFailure(activity, e);

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

    private async Task<PooledHost> start(AlbaHostSlot slot, CancellationToken cancellationToken)
    {
        if (_starting is not null) await _starting.WaitAsync(cancellationToken);

        (IAlbaHost Host, TState State)? started = null;
        PooledHost? pooled = null;
        try
        {
            started = await _factory(slot, cancellationToken);
            if (started.Value.Host is not AlbaHost host)
            {
                throw new InvalidOperationException(
                    "The AlbaHostPool factory must return a host started through AlbaHost.For(...)");
            }

            pooled = new PooledHost(host, started.Value.State, slot);
            claimResettableExtensions(pooled);

            if (_options.OnCreated is { } onCreated) await onCreated(host, started.Value.State);

            pooled.Checkpoint();
            return pooled;
        }
        catch
        {
            await AlbaHost.DisposeQuietly(() => pooled is not null
                ? destroy(pooled)
                : dispose(started?.Host, started.HasValue ? started.Value.State : null, slot));
            throw;
        }
        finally
        {
            _starting?.Release();
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

    private async Task ReturnAsync(AlbaHostLease lease)
    {
        var pooled = lease.Pooled;
        lock (_lock) _leases.Remove(lease);

        using var activity = AlbaTracing.StartPoolActivity("return");
        activity?.SetTag(AlbaTracing.PoolSlot, pooled.Slot.Index);
        activity?.SetTag(AlbaTracing.PoolDiscarded, lease.Discarded);

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

            if (_options.OnReturn is { } onReturn) await onReturn(pooled.Host, stateOf(pooled));
        }
        catch (Exception e)
        {
            AlbaTracing.RecordFailure(activity, e);
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
                Volatile.Write(ref _lastReturn, Environment.TickCount64);

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
        Volatile.Write(ref _lastReturn, Environment.TickCount64);

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
        return dispose(pooled.Host, pooled.State, pooled.Slot);
    }

    // The state goes first, because it may use its host while it is disposed
    private static Task dispose(IAlbaHost? host, object? state, AlbaHostSlot slot)
    {
        return Teardown.RunAllAsync($"One or more failures while disposing the host in {slot.Name}",
            [() => disposeState(state), () => host?.DisposeAsync().AsTask() ?? Task.CompletedTask, slot.DisposeResourcesAsync]);
    }

    private static async Task disposeState(object? state)
    {
        if (state is IAsyncDisposable asyncDisposable) await asyncDisposable.DisposeAsync();
        else if (state is IDisposable disposable) disposable.Dispose();
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
}
