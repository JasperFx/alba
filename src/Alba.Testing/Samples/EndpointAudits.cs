using Shouldly;

namespace Alba.Testing.Samples;

#region sample_endpoint_coverage_fixture
public class ApiCoverage : IAsyncLifetime
{
    // One instance for every host of the application, however many fixtures or pools create them
    public static readonly EndpointCoverage Coverage = new();

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    public ValueTask DisposeAsync()
    {
        // As an assembly fixture, this runs after the last test in the assembly.
        // The exception fails the test run, naming each endpoint that no test reached.
        Coverage.NotExercised().ShouldBeEmpty();
        return ValueTask.CompletedTask;
    }
}

public class ApiFixture : IAsyncLifetime
{
    public IAlbaHost Host = null!;

    public async ValueTask InitializeAsync()
    {
        Host = await AlbaHost.For<WebApp.Program>(ApiCoverage.Coverage);
    }

    public async ValueTask DisposeAsync()
    {
        await Host.DisposeAsync();
    }
}
#endregion

public class EndpointAudits
{
    #region sample_endpoints_allowing_anonymous
    [Fact]
    public async Task no_endpoint_allows_anonymous_requests()
    {
        await using var host = await AlbaHost.For<WebAppSecuredWithJwt.Program>();

        // A new endpoint that forgets [Authorize] or RequireAuthorization() fails this test
        host.EndpointsAllowingAnonymous().ShouldBeEmpty();
    }
    #endregion
}
