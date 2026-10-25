using Alba;
using Alba.Pooling;
using TUnit.Core.Interfaces;

namespace TUnitSamples;

#region sample_TUnit_host_pool
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
#endregion

#region sample_TUnit_pooled_tests
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
#endregion
