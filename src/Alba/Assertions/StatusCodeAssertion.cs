using Microsoft.AspNetCore.Http;

namespace Alba.Assertions;

#region sample_StatusCodeAssertion
internal sealed class StatusCodeAssertion : IScenarioAssertion
{
    public int Expected { get; set; }

    public StatusCodeAssertion(int expected)
    {
        Expected = expected;
    }

    public void Assert(Scenario scenario, HttpContext context, ScenarioAssertionException ex)
    {
        var statusCode = context.Response.StatusCode;
        if (statusCode != Expected)
        {
            ex.Add($"Expected status code {Expected}, but was {statusCode}");

            ex.ReadBody(context);
        }
    }
}
#endregion