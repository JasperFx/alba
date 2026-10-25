# Integrating with NUnit

When using Alba within NUnit testing projects, you probably want to reuse the `IAlbaHost` across tests and test fixtures because
`AlbaHost` is relatively expensive to create (it's bootstrapping your whole application more than Alba itself is slow). To do that with NUnit, you could
track a single `AlbaHost` on a static class like this one:

<!-- snippet: sample_NUnit_Application -->
<a id='snippet-sample_NUnit_Application'></a>
```cs
[SetUpFixture]
public class Application
{
    [OneTimeSetUp]
    public async Task Init()
    {
        Host = await AlbaHost.For<WebApp.Program>();
    }
        
    public static IAlbaHost Host { get; private set; }

    // Make sure that NUnit will shut down the AlbaHost when
    // all the projects are finished
    [OneTimeTearDown]
    public void Teardown()
    {
        Host.Dispose();
    }
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/NUnitSamples/UnitTest1.cs#L7-L29' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_NUnit_Application' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Any Alba specific setup, such as configuration overrides or extensions, is chained onto `AlbaHost.For<T>()` before it is awaited
(see [Configuring the Host Fluently](gettingstarted.md#configuring-the-host-fluently)).

Then reference the `AlbaHost` in tests like this sample:

<!-- snippet: sample_NUnit_scenario_test -->
<a id='snippet-sample_NUnit_scenario_test'></a>
```cs
public class sample_integration_fixture
{
    [Test]
    public async Task happy_path()
    {
        await Application.Host.Scenario(_ =>
        {
            _.Get.Url("/fake/okay");
            _.StatusCodeShouldBeOk();
        });
    }
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/NUnitSamples/UnitTest1.cs#L31-L44' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_NUnit_scenario_test' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

## Running tests in parallel with a host pool

Tests that change data can't safely share one host while they run in parallel. An [`AlbaHostPool`](pooling.md) lends
each test a host of its own. Create the pool in a `[SetUpFixture]`, and dispose it in its `[OneTimeTearDown]`:

<!-- snippet: sample_NUnit_host_pool -->
<a id='snippet-sample_NUnit_host_pool'></a>
```cs
// Applies to the tests in this namespace
[SetUpFixture]
public class PooledApplication
{
    public static AlbaHostPool Pool { get; private set; }

    [OneTimeSetUp]
    public void CreatePool()
    {
        Pool = new AlbaHostPool(slot => AlbaHost.For<WebApp.Program>()
                // Each host gets its own database
                .WithConfiguration("ConnectionStrings:Db", $"Host=localhost;Database=app_{slot.Index}"),
            new AlbaHostPoolOptions
            {
                // Hosts start as tests need them, up to this many
                MaxHosts = 4
            });
    }

    // Runs after every test in the namespace has finished
    [OneTimeTearDown]
    public async Task DisposePool()
    {
        await Pool.DisposeAsync();
    }
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/NUnitSamples/HostPooling.cs#L8-L37' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_NUnit_host_pool' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Then rent a host in `[SetUp]` and return it in `[TearDown]`. NUnit runs tests one at a time unless they are marked
`[Parallelizable]`, and it shares one fixture instance between a class's tests unless the class uses
`[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]`. Without that attribute, tests running in parallel would overwrite
each other's lease:

<!-- snippet: sample_NUnit_pooled_tests -->
<a id='snippet-sample_NUnit_pooled_tests'></a>
```cs
// A new instance for each test, so tests running in parallel each keep their own lease
[FixtureLifeCycle(LifeCycle.InstancePerTestCase)]
[Parallelizable(ParallelScope.Children)]
public class pooled_math_tests
{
    private AlbaHostLease _host;

    [SetUp]
    public async Task RentHost()
    {
        // Name the test, because timeout messages would otherwise name this method
        _host = await PooledApplication.Pool.RentAsync(TestContext.CurrentContext.CancellationToken,
            caller: TestContext.CurrentContext.Test.Name);
    }

    // Resets the host and returns it to the pool
    [TearDown]
    public async Task ReturnHost()
    {
        if (_host != null) await _host.DisposeAsync();
    }

    [TestCase(1, 2, 3)]
    [TestCase(2, 3, 5)]
    [TestCase(10, 20, 30)]
    public async Task adds_numbers(int one, int two, int sum)
    {
        await _host.Scenario(x =>
        {
            x.Get.Url($"/math/add/{one}/{two}");
            x.JsonPath("$.answer").ShouldBe(sum);
        });
    }
}
```
<sup><a href='https://github.com/JasperFx/alba/blob/master/src/NUnitSamples/HostPooling.cs#L39-L76' title='Snippet source file'>snippet source</a> | <a href='#snippet-sample_NUnit_pooled_tests' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->
