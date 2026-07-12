using Shouldly;

namespace Alba.Testing.Acceptance;

public class http_query_method
{
    [Fact]
    public async Task runs_a_query_request_with_a_body()
    {
        await using var host = await AlbaHost.For<MinimalApiWithOakton.Program>();

        var result = await host.Scenario(_ =>
        {
            _.Query.Url("/api/query");
            _.Body.TextIs("Blue");

            _.ContentShouldBe("I ran a QUERY with value Blue");
        });

        result.Context.Request.Method.ShouldBe("QUERY");
    }
}
