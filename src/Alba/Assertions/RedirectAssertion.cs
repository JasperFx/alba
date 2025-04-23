namespace Alba.Assertions;

internal sealed class RedirectAssertion : IScenarioAssertion
{
    public RedirectAssertion(string expected, bool permanent)
    {
        Expected = expected;
        Permanent = permanent;
    }

    public string Expected { get; }
    public bool Permanent { get; }

    public void Assert(Scenario scenario, AssertionContext context)
    {
        var location = context.HttpContext.Response.Headers.Location;
        if (!string.Equals(location, Expected, StringComparison.OrdinalIgnoreCase))
        {
            context.AddFailure($"Expected to be redirected to '{Expected}' but was '{location}'.");
        }

        new StatusCodeAssertion(Permanent ? 301 : 302).Assert(scenario, context);
    }
}