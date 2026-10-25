using Alba.Extensions;
using Alba.Pooling;
using Shouldly;

[assembly: AssemblyFixture<Alba.Testing.Samples.ApiHosts>]

namespace Alba.Testing.Samples;

#region sample_host_pool_fixture
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
#endregion

#region sample_renting_a_pooled_host
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
#endregion
