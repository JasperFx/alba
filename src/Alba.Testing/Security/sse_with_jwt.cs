using System.Security.Claims;
using Alba.Security;
using Shouldly;

namespace Alba.Testing.Security;

public class sse_with_jwt : IAsyncLifetime
{
    private IAlbaHost theHost = null!;

    public async ValueTask InitializeAsync()
    {
        var stub = new JwtSecurityStub().With("foo", "bar");
        theHost = await AlbaHost.For<WebAppSecuredWithJwt.Program>(stub);
    }

    public async ValueTask DisposeAsync()
    {
        await theHost.DisposeAsync();
    }

    [Fact]
    public async Task can_stream_from_a_secured_endpoint()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        await using var stream = await theHost.StreamServerSentEvents(
            x => x.Get.Url("/sse/secured"), timeout.Token);

        var events = new List<string>();
        await foreach (var item in stream.ReadEvents(timeout.Token))
        {
            events.Add(item.Data);
        }

        events.ShouldBe(new[] { "bar-1", "bar-2" });
    }

    [Fact]
    public async Task per_scenario_claims_flow_through_the_stream()
    {
        var stub = new JwtSecurityStub();
        await using var host = await AlbaHost.For<WebAppSecuredWithJwt.Program>(stub);

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        await using var stream = await host.StreamServerSentEvents(x =>
        {
            x.WithClaim(new Claim("foo", "streamed"));
            x.Get.Url("/sse/secured");
        }, timeout.Token);

        var events = new List<string>();
        await foreach (var item in stream.ReadEvents(timeout.Token))
        {
            events.Add(item.Data);
        }

        events.ShouldBe(new[] { "streamed-1", "streamed-2" });
    }

    [Fact]
    public async Task missing_token_surfaces_as_a_status_code_failure()
    {
        var ex = await Should.ThrowAsync<ScenarioAssertionException>(
            () => theHost.StreamServerSentEvents(x =>
            {
                x.RemoveRequestHeader("Authorization");
                x.Get.Url("/sse/secured");
            }));

        ex.Message.ShouldContain("Expected a status code between 200 and 299, but was 401");
    }
}
