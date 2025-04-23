namespace Alba.Assertions;

internal sealed  class HeaderExistsAssertion : IScenarioAssertion
{
    private readonly string _headerKey;

    public HeaderExistsAssertion(string headerKey)
    {
        _headerKey = headerKey;
    }

    public void Assert(Scenario scenario, AssertionContext context)
    {
        var values = context.HttpContext.Response.Headers[_headerKey];

        if (values.Count == 0)
        {
            context.AddFailure($"Expected header '{_headerKey}' to be present but no values were found on the response.");
        }

    }
}