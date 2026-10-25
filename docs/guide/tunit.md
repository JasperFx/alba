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
    public AlbaHostPool Pool { get; } = new(slot => AlbaHost.For<WebApp.Program>()
            // Each host gets its own database
            .WithConfiguration("ConnectionStrings:Db", $"Host=localhost;Database=app_{slot.Index}"),
        new AlbaHostPoolOptions
        {
            // Hosts start as tests need them, up to this many. TUnit runs every test in parallel,
            // so the rest wait here for a host.
            MaxHosts = 4
        });

    public ValueTask DisposeAsync() => Pool.DisposeAsync();
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/TUnitSamples/HostPooling.cs#L7-L22' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_TUnit_host_pool' title='Start of snippet'>anchor</a></sup>
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
        // Name the test, because timeout messages would otherwise name this method
        _lease = await pool.Pool.RentAsync(caller: TestContext.Current?.Metadata.TestName ?? "");
    }

    // Resets the host and returns it to the pool
    public async ValueTask DisposeAsync()
    {
        if (_lease is not null) await _lease.DisposeAsync();
    }
}

[ClassDataSource<AlbaPool>(Shared = SharedType.PerTestSession)]
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
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/TUnitSamples/HostPooling.cs#L24-L61' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_TUnit_pooled_tests' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

TUnit starts more tests than the pool has hosts, and the extra tests wait for one. The pool's rent timeout only fires
when no host is returned for the whole timeout, so a long queue doesn't fail while tests keep finishing.
