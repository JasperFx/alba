namespace Alba.Assertions;

#region sample_StatusCodeAssertion
internal sealed class StatusCodeAssertion : IScenarioAssertion, IStatusCodeAssertion
{
    public int Expected { get; set; }

    public StatusCodeAssertion(int expected)
    {
        Expected = expected;
    }

    public void Assert(Scenario scenario, AssertionContext context)
    {
        var statusCode = context.HttpContext.Response.StatusCode;
        if (statusCode != Expected)
        {
            context.AddFailure($"Expected status code {Expected}, but was {statusCode}");

            context.ReadBodyAsString();
        }
    }
}
#endregion