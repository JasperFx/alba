using System.Threading.Tasks;
using Alba;
using Alba.Pooling;
using NUnit.Framework;

namespace NUnitSamples.Pooling;

#region sample_NUnit_host_pool

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

#endregion

#region sample_NUnit_pooled_tests

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

#endregion
