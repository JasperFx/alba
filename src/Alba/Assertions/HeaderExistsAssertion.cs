using Microsoft.AspNetCore.Http;

namespace Alba.Assertions;

internal sealed  class HeaderExistsAssertion : IScenarioAssertion
{
    private readonly string _headerKey;

    public HeaderExistsAssertion(string headerKey)
    {
        _headerKey = headerKey;
    }

    public void Assert(Scenario scenario, HttpContext context, ScenarioAssertionException ex)
    {
        var values = context.Response.Headers[_headerKey];

        if (values.Count == 0)
        {
            ex.Add($"Expected header '{_headerKey}' to be present but no values were found on the response.");
        }

    }
}