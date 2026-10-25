# Pooling Hosts for Parallel Tests

Sharing one `AlbaHost` keeps a test suite fast, but tests that change data or stubbed responses can't run against it at
the same time. `AlbaHostPool` runs several hosts of the application and lends each one to a single test at a time. Tests
run in parallel, each one has a host to itself, and every host is reset before the next test rents it.

## Creating a pool

The pool calls a factory each time it needs a host. The factory gets the host's `AlbaHostSlot`, whose `Index` runs
from 0 to one less than `MaxHosts`, so each host can be given its own database or other infrastructure. The pool types
are in the `Alba.Pooling` namespace.

<!-- snippet: sample_host_pool_fixture -->
<a id='snippet-sample_host_pool_fixture'></a>
```cs
// Registered once per test assembly with [assembly: AssemblyFixture<ApiHosts>]
public class ApiHosts : IAsyncLifetime
{
    public AlbaHostPool Pool { get; } = new(slot => AlbaHost.For<global::Program>()
            // Each host gets its own database and its own stub for outbound calls
            .WithConfiguration("ConnectionStrings:Db", $"Host=localhost;Database=app_{slot.Index}")
            .WithExtension(new HttpClientStub("payments")),
        new AlbaHostPoolOptions
        {
            // Hosts start as tests need them, up to this many
            MaxHosts = 4
        });

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public ValueTask DisposeAsync() => Pool.DisposeAsync();
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/HostPooling.cs#L8-L26' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_host_pool_fixture' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Hosts start as tests need them, so a suite that never runs more than two tests at once only starts two hosts.
`MaxHosts` defaults to the processor count, which is also xUnit's default number of tests that run in parallel. Each
host holds a whole application, with its own caches and connection pools, plus whatever runs per host such as a
database. For a heavy application, start with half the processor count and measure. 

Create one pool for the test assembly and dispose it after the last test:

- **xUnit**: an assembly fixture, as above, registered with `[assembly: AssemblyFixture<ApiHosts>]` and taken as a
  constructor argument by the test classes.
- **NUnit**: a static pool created in the `[OneTimeSetUp]` of a `[SetUpFixture]` and disposed in its `[OneTimeTearDown]`.
  NUnit runs tests one at a time unless they are marked `[Parallelizable]`. See the
  [NUnit example](nunit.md#running-tests-in-parallel-with-a-host-pool).
- **TUnit**: a class that owns the pool and implements `IAsyncDisposable`, injected with
  `[ClassDataSource<ApiHosts>(Shared = SharedType.PerTestSession)]`. See the
  [TUnit example](tunit.md#isolating-parallel-tests-with-a-host-pool).

## Renting a host

A test rents a host, uses it alone, and disposes the lease to return it:

<!-- snippet: sample_renting_a_pooled_host -->
<a id='snippet-sample_renting_a_pooled_host'></a>
```cs
public class PooledHostTests(ApiHosts hosts)
{
    [Fact]
    public async Task say_hello()
    {
        // Disposing the lease resets the host and returns it to the pool
        await using var host = await hosts.Pool.RentAsync(TestContext.Current.CancellationToken);

        await host.Scenario(x =>
        {
            x.Get.Url("/");
            x.ContentShouldBe("Hello World!");
        });

        // This host's own stub, separate from the stubs of the other hosts
        host.GetExtension<HttpClientStub>().Requests.ShouldBeEmpty();
    }
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/Alba.Testing/Samples/HostPooling.cs#L28-L47' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_renting_a_pooled_host' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

The lease is an `IAlbaHost`, so scenarios and helpers such as `GetAsJson` work unchanged. Always dispose it with
`await using`. A lease that is never disposed prevents other tests from using that host.

- `BeforeEach`, `AfterEach` and their async versions called on the lease only apply until the lease is disposed. Actions
  that every test needs belong in the factory or in `OnCreated`.
- `host.GetExtension<HttpClientStub>()` finds the rented host's own extension. Pass a match, such as
  `x => x.ClientName == "payments"`, when the host has several of the same type.
- `host.Slot` is the rented host's slot, for reaching its own resources from the test.
- Using a lease after it is disposed throws an `ObjectDisposedException`.

## Waiting for a host

When every host is rented, `RentAsync` waits for one to be returned. Waiting tests get hosts in the order they asked.

- `RentTimeout` (two minutes by default) fails a rent when no host has been returned to the pool for that long. The wait
  starts again each time any host comes back, so a long queue of tests, such as TUnit starting every test at once, does
  not time out while the pool keeps moving. The `TimeoutException` names each lease not yet returned and the test that
  rented it, which finds a test that forgot to dispose its lease.
- The test is named from the method that calls `RentAsync`. When a base class or fixture rents for each test, pass the
  test's name instead, such as `pool.RentAsync(cancellation, caller: TestContext.Current.Test?.TestDisplayName ?? "")`
  in xUnit.
- `RentAsync` takes a cancellation token, such as xUnit's `TestContext.Current.CancellationToken`.
- Rent one host per test. A test that rents a second host while holding one can wait forever when the pool is full.
- `WarmUpAsync()` starts hosts in parallel ahead of the first tests, so a startup failure surfaces in the fixture rather
  than in a test.
- `MaxConcurrentStarts` limits how many hosts start at once. Set it to 1 when the application's startup code can't run
  for several hosts in one process at the same time, for example because it creates databases or takes a lock in a
  shared scheduler.

When a host fails to start, the test that started it gets the exception and every later rent fails with an
`InvalidOperationException` wrapping it, so a broken application fails the suite once rather than once per test. Set
`FailFastOnStartupFailure = false` to try again on each rent instead.

## Resetting between tests

Disposing a lease resets the host in this order:

1. `BeforeEach` and `AfterEach` actions added since `OnCreated` ran, through the lease or in `OnRent`, are removed.
2. Extensions that implement `IResettableAlbaExtension` go back to their state once `OnCreated` ran. `HttpClientStub`
   clears its recorded requests and keeps only the responses stubbed in the factory or in `OnCreated`.
3. `OnReturn` runs, to remove the data the test created.

```cs
new AlbaHostPoolOptions
{
    // Once per host, before its first test
    OnCreated = async (host, slot) =>
    {
        using var scope = host.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.MigrateAsync();
    },

    // After every test, for example with Marten
    OnReturn = (host, slot) => host.ResetAllMartenDataAsync()
}
```

`OnRent` runs before each test gets the host, for example to seed data. When `OnRent` or `OnReturn` throws, the pool
disposes that host and starts a new one in the same slot when it is next needed. A failed `OnReturn` is thrown from
disposing the lease, so the test that left the host in a bad state is the one that fails.

Call `host.Discard()` when a test changes something that resetting does not undo, such as a singleton's state. The pool
disposes that host when the lease is disposed and replaces it in the same slot.

Tests that create unique data for themselves, such as new ids or tenants, may not need any reset. That is the cheapest
option when the application allows it.

`TimeProviderOverride` is not reset, because its clock can only move forward. Tests that depend on the current time
should set it with `SetUtcNow(...)` to a time later than any earlier test used.

A test harness that is started once per host and reused, such as MassTransit's `ITestHarness`, keeps what it recorded
from earlier tests. Look for messages by values the current test created, and raise inactivity timeouts such as
MassTransit's `TestHarnessOptions.TestInactivityTimeout` past the length of the test run, because once they expire they
don't reset.

## Keeping state with each host

Tests often need more than the host: a mock that isn't registered in DI, a `DbContext` pointed at the host's database,
or users seeded when the host started. `AlbaHostPool<TState>` keeps an object of your own with each host. The factory
returns the host and its state, the lifecycle actions receive the state, and `lease.State` gives it to the test:

```cs
public sealed class ApiInstance(string connectionString) : IAsyncDisposable
{
    public string ConnectionString { get; } = connectionString;
    public Mock<IChatClient> ChatClient { get; } = new();
    public Profile Administrator { get; set; } = null!;

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

var pool = new AlbaHostPool<ApiInstance>(async (slot, cancellation) =>
{
    var instance = new ApiInstance(await TestDatabases.CreateAsync($"app_{slot.Index}", cancellation));

    var host = await AlbaHost.For<Program>(x => x.ConfigureServices(s => s.AddSingleton(instance.ChatClient.Object)))
        .WithConfiguration("ConnectionStrings:Db", instance.ConnectionString);

    return (host, instance);
}, new AlbaHostPoolOptions<ApiInstance>
{
    OnCreated = async (host, instance) => instance.Administrator = await Seed.AdministratorAsync(host),
    OnRent = (host, instance) => TestDatabases.RestoreSeedAsync(instance.ConnectionString)
});

await using var host = await pool.RentAsync(cancellation);
host.State.ChatClient.Setup(...);
```

When the state implements `IAsyncDisposable` or `IDisposable`, the pool disposes it just before its host. The pool
disposes the host itself, so the state doesn't need to.

## Separate infrastructure per host

Derive each host's resources from its slot. A replacement host gets the same index, so it reuses the same resources.

| Resource | Separate it by |
|---|---|
| Relational database | One database per slot on a shared server or container, such as `app_{slot.Index}` |
| Marten or EF Core | One schema per slot, which is cheaper than separate databases |
| Message broker | A queue or topic prefix per slot, from `slot.Name` |
| Files | A directory per slot |

When resources must exist before the host starts, use the factory that returns the started host:

```cs
var pool = new AlbaHostPool(async (slot, cancellation) =>
{
    var database = $"app_{slot.Index}";
    await TestDatabases.CreateAsync(database, cancellation);

    // Disposed after this host is disposed
    slot.RegisterForDispose(new DropDatabase(database));

    return await AlbaHost.For<Program>()
        .WithConfiguration("ConnectionStrings:Db", TestDatabases.ConnectionStringFor(database));
});
```

The pool runs the factory again when it replaces a host: after `Discard()`, after a failed `OnRent`, `OnReturn` or
extension reset, and after a startup failure when `FailFastOnStartupFailure` is off. So the factory must work when run
more than once for the same slot. Registering the cleanup with `slot.RegisterForDispose` is enough when the cleanup can't fail. Otherwise
create the resource only if it is missing, or name it with `slot.Generation`, which counts the hosts the slot held
before, such as `app_{slot.Index}_{slot.Generation}`.

Create extensions that hold state for one host, such as `HttpClientStub`, inside the factory. The pool throws if one
`IResettableAlbaExtension` instance is passed to two hosts, because returning one host would reset the other. Extensions
meant to be shared, such as [`EndpointCoverage`](endpoints.md), can be passed to every host.

Hosts in a pool share one process, so static state in the application, such as a static cache or Serilog's
`Log.Logger`, is shared by every host. So is anything the application reads from process-wide sources, such as
environment variables or a `.env` file loaded after the test configuration. Give each host its own value through
`WithConfiguration` or a service registration in the factory instead.

## Tracing the pool

The pool records activities on Alba's `"Alba"` `ActivitySource`, alongside the scenario activities described in
[Tracing & Open Telemetry](opentelemetry.md):

| Activity | Covers | Tags |
|---|---|---|
| `AlbaHostPool rent` | A rent, from waiting for a host to `OnRent` | `alba.pool.slot`, `alba.pool.wait_ms` |
| `AlbaHostPool start host` | Starting a host, including `OnCreated` | `alba.pool.slot`, `alba.pool.slot_generation` |
| `AlbaHostPool return` | Resetting a returned host, including `OnReturn` | `alba.pool.slot`, `alba.pool.discarded` |

They show where a slow suite spends its time: waiting for hosts, starting them or resetting them. A failure marks the
activity with an error status.
