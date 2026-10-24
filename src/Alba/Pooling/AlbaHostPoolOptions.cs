namespace Alba.Pooling;

/// <summary>
/// Limits and lifecycle actions for an AlbaHostPool
/// </summary>
public sealed class AlbaHostPoolOptions
{
    /// <summary>
    /// The most hosts the pool runs at once. Hosts start as tests need them, so a suite that
    /// never runs more than two tests at once only starts two. Defaults to the processor count,
    /// which is also xUnit's default number of tests that run in parallel.
    /// </summary>
    public int MaxHosts { get; set; } = Environment.ProcessorCount;

    /// <summary>
    /// How long RentAsync waits for a host to be returned when every host is rented. Starting
    /// a host does not count against it. Defaults to two minutes.
    /// </summary>
    public TimeSpan RentTimeout { get; set; } = TimeSpan.FromMinutes(2);

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
    public Func<IAlbaHost, AlbaHostSlot, Task>? OnCreated { get; set; }

    /// <summary>
    /// Runs each time a host is rented, before RentAsync returns. A failure replaces the host
    /// and fails the rent.
    /// </summary>
    public Func<IAlbaHost, AlbaHostSlot, Task>? OnRent { get; set; }

    /// <summary>
    /// Runs each time a lease is disposed, after the host's resettable extensions are reset,
    /// for example to delete the data a test created. A failure replaces the host and is
    /// thrown from disposing the lease.
    /// </summary>
    public Func<IAlbaHost, AlbaHostSlot, Task>? OnReturn { get; set; }
}
