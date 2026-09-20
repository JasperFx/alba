namespace Alba.Assertions;

internal sealed class RedirectAssertion : IScenarioAssertion
{
    public RedirectAssertion(string expected)
    {
        Expected = expected;
    }

    public string Expected { get; }

    public void Assert(Scenario scenario, AssertionContext context)
    {
        var location = context.HttpContext.Response.Headers.Location;
        if (!string.Equals(location, Expected, StringComparison.OrdinalIgnoreCase))
        {
            context.AddFailure($"Expected to be redirected to '{Expected}' but was '{location}'.");
        }
    }
}
