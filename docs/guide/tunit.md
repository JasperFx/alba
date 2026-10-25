# Integrating with TUnit

Like other testing frameworks, you'll want to reuse the `IAlbaHost` across tests and test fixtures because
`AlbaHost` is relatively expensive to create. To do that with TUnit, you should start by writing a bootstrapping class
 that inherits from `IAsyncInitializer` and `IAsyncDisposable`:

<!-- snippet: sample_TUnit_Application -->
<a id='snippet-sample_TUnit_Application'></a>
```cs
public sealed class AlbaBootstrap : IAsyncInitializer, IAsyncDisposable
{
    public IAlbaHost Host { get; private set; } = null!;

    public async Task InitializeAsync() 
    {
        Host = await AlbaHost.For<WebApp.Program>();
    }

    public async ValueTask DisposeAsync()
    {
        await Host.DisposeAsync();
    }
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/TUnitSamples/Program.cs#L6-L21' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_TUnit_Application' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Any Alba specific setup, such as configuration overrides or extensions, is chained onto `AlbaHost.For<T>()` before it is awaited
(see [Configuring the Host Fluently](gettingstarted.md#configuring-the-host-fluently)).

Then inject the instance by adding `[ClassDataSource<AlbaBootstrap>(Shared = SharedType.PerTestSession)]` to your test class. We recommend creating a base class to allow easier access of the host and any other dependencies.

<!-- snippet: sample_TUnit_scenario_test -->
<a id='snippet-sample_TUnit_scenario_test'></a>
```cs
public abstract class AlbaTestBase(AlbaBootstrap albaBootstrap)
{
    protected IAlbaHost Host => albaBootstrap.Host;
}

[ClassDataSource<AlbaBootstrap>(Shared = SharedType.PerTestSession)]
public class MyTestClass(AlbaBootstrap albaBootstrap) : AlbaTestBase(albaBootstrap)
{
    [Test]
    public async Task happy_path()
    {
        await Host.Scenario(_ =>
        {
            _.Get.Url("/fake/okay");
            _.StatusCodeShouldBeOk();
        });
    }
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/TUnitSamples/Program.cs#L23-L42' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_TUnit_scenario_test' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Isolating parallel tests with a host pool

TUnit runs tests in parallel by default, so tests that change data can't safely share one host. An
[`AlbaHostPool`](pooling.md) lends each test a host of its own. Share the pool for the test session:

<!-- snippet: sample_TUnit_host_pool -->
<a id='snippet-sample_TUnit_host_pool'></a>
```cs
public sealed class AlbaPool : IAsyncDisposable
{
    public const int Hosts = 4;

    public AlbaHostPool Pool { get; } = new(slot => AlbaHost.For<WebApp.Program>()
            // Each host gets its own database
            .WithConfiguration("ConnectionStrings:Db", $"Host=localhost;Database=app_{slot.Index}"),
        new AlbaHostPoolOptions
        {
            // Hosts start as tests need them, up to this many
            MaxHosts = Hosts
        });

    public ValueTask DisposeAsync() => Pool.DisposeAsync();
}

// TUnit runs no more of the tests marked with this limiter at once than the pool has hosts
public sealed class PooledHostsLimit : IParallelLimit
{
    public int Limit => AlbaPool.Hosts;
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/TUnitSamples/HostPooling.cs#L7-L29' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_TUnit_host_pool' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Then rent a host for each test. TUnit creates the test class for each test, so a base class can rent in
`InitializeAsync` and return the host in `DisposeAsync`:

<!-- snippet: sample_TUnit_pooled_tests -->
<a id='snippet-sample_TUnit_pooled_tests'></a>
```cs
// TUnit creates the test class for each test, so each test rents its own host
public abstract class PooledTestBase(AlbaPool pool) : IAsyncInitializer, IAsyncDisposable
{
    private AlbaHostLease? _lease;

    protected IAlbaHost Host => _lease!;

    public async Task InitializeAsync()
    {
        var test = TestContext.Current;

        // Name the test, because timeout messages would otherwise name this method
        _lease = await pool.Pool.RentAsync(test?.Execution.CancellationToken ?? default,
            caller: test?.Metadata.TestName ?? "");
    }

    // Resets the host and returns it to the pool
    public async ValueTask DisposeAsync()
    {
        if (_lease is not null) await _lease.DisposeAsync();
    }
}

[ClassDataSource<AlbaPool>(Shared = SharedType.PerTestSession)]
[ParallelLimiter<PooledHostsLimit>]
public class PooledMathTests(AlbaPool pool) : PooledTestBase(pool)
{
    [Test]
    [Arguments(1, 2, 3)]
    [Arguments(2, 3, 5)]
    [Arguments(10, 20, 30)]
    public async Task adds_numbers(int one, int two, int sum)
    {
        await Host.Scenario(x =>
        {
            x.Get.Url($"/math/add/{one}/{two}");
            x.JsonPath("$.answer").ShouldBe(sum);
        });
    }
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/TUnitSamples/HostPooling.cs#L31-L72' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_TUnit_pooled_tests' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

`[ParallelLimiter<PooledHostsLimit>]` stops TUnit running more of these tests at once than the pool has hosts, so a
test doesn't start only to wait for a host. The limit is shared by every class marked with the same limiter type.

TUnit disposes the `AlbaPool`, and with it every host, after the test session ends.

Shared infrastructure such as a database container can be injected into the class that owns the pool, as a property
with its own `[ClassDataSource]`. TUnit initializes it before the owning class, so create the pool in
`InitializeAsync`:

```cs
public sealed class AlbaPool : IAsyncInitializer, IAsyncDisposable
{
    // Startup the containers before the test pool
    [ClassDataSource<PostgresContainer>(Shared = SharedType.PerTestSession)]
    public required PostgresContainer Database { get; init; }

    public AlbaHostPool Pool { get; private set; } = null!;

    public Task InitializeAsync()
    {
        Pool = new AlbaHostPool(slot => AlbaHost.For<WebApp.Program>()
                .WithConfiguration("ConnectionStrings:Db", Database.ConnectionStringFor($"app_{slot.Index}")),
            new AlbaHostPoolOptions { MaxHosts = 4 });

        return Task.CompletedTask;
    }

    public ValueTask DisposeAsync() => Pool.DisposeAsync();
}
```

Tests that create their own data instead of relying on a reset can name it with
`TestContext.Current!.Isolation.GetIsolatedPrefix()`, which is unique to each test, for example as a tenant id.

## Application output and logs

Output the application writes with `Console` while it handles a scenario appears in the output of the test that sent
the request. TUnit traces each test, Alba sends that trace with the request, and TUnit uses it to find the test.

The default console logger writes from a background thread, so its messages don't reach the test. To see the
application's `ILogger` output in each test, add the `TUnit.Logging.Microsoft` package and register its logger in the
host's services:

<!-- snippet: sample_TUnit_correlated_logging -->
<a id='snippet-sample_TUnit_correlated_logging'></a>
```cs
// Writes the application's logs to the test that sent the request
builder.Services.AddCorrelatedTUnitLogging();
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/TUnitSamples/ApplicationOutput.cs#L32-L35' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_TUnit_correlated_logging' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

