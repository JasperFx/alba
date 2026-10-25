namespace Alba.Pooling;

/// <summary>
/// Limits and lifecycle actions for an AlbaHostPool&lt;TState&gt;. The actions receive each host
/// with the state the pool's factory created for it.
/// </summary>
/// <typeparam name="TState"></typeparam>
public class AlbaHostPoolOptions<TState>
{
    /// <summary>
    /// The most hosts the pool runs at once. Hosts start as tests need them, so a suite that
    /// never runs more than two tests at once only starts two. Defaults to the processor count,
    /// which is also xUnit's default number of tests that run in parallel.
    /// </summary>
    public int MaxHosts { get; set; } = Environment.ProcessorCount;

    /// <summary>
    /// How long a rent waits while no host is returned to the pool. The wait starts again each
    /// time a host is returned, so a long queue of tests does not time out while the pool keeps
    /// moving. Defaults to two minutes.
    /// </summary>
    public TimeSpan RentTimeout { get; set; } = TimeSpan.FromMinutes(2);

    /// <summary>
    /// The most hosts that start at once, including OnCreated. Null, the default, starts every
    /// host that is needed at once. Use 1 for an application that can't start several hosts
    /// in one process at the same time.
    /// </summary>
    public int? MaxConcurrentStarts { get; set; }

    /// <summary>
    /// When a host fails to start, fail every later rent with that failure instead of starting
    /// another host, so a broken application fails the suite once rather than once per test.
    /// On by default.
    /// </summary>
    public bool FailFastOnStartupFailure { get; set; } = true;

    /// <summary>
    /// Runs once for each host after it starts and before it is first rented, for example to
    /// migrate the host's database. A failure counts as the host failing to start.
    /// </summary>
    public Func<IAlbaHost, TState, Task>? OnCreated { get; set; }

    /// <summary>
    /// Runs each time a host is rented, before RentAsync returns. A failure replaces the host
    /// and fails the rent.
    /// </summary>
    public Func<IAlbaHost, TState, Task>? OnRent { get; set; }

    /// <summary>
    /// Runs each time a lease is disposed, after the host's resettable extensions are reset,
    /// for example to delete the data a test created. A failure replaces the host and is
    /// thrown from disposing the lease.
    /// </summary>
    public Func<IAlbaHost, TState, Task>? OnReturn { get; set; }
}

/// <summary>
/// Limits and lifecycle actions for an AlbaHostPool. The actions receive each host with its slot.
/// </summary>
public sealed class AlbaHostPoolOptions : AlbaHostPoolOptions<AlbaHostSlot>;
