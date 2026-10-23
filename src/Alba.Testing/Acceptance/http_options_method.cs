using System.Net;
using Shouldly;

namespace Alba.Testing.Acceptance;

public class http_options_method
{
    [Fact]
    public async Task runs_an_options_request()
    {
        await using var host = await AlbaHost.For<MinimalApiWithOakton.Program>();

        var result = await host.Scenario(_ =>
        {
            _.Options.Url("/api/options");

            _.StatusCodeShouldBe(HttpStatusCode.NoContent);
            _.Header("Allow").SingleValueShouldEqual("GET, POST, OPTIONS");
        });

        result.Context.Request.Method.ShouldBe("OPTIONS");
    }
}
