namespace Alba.Assertions;

#region sample_StatusCodeAssertion
internal sealed class StatusCodeAssertion : IScenarioAssertion
{
    public int Expected { get; set; }

    public StatusCodeAssertion(int expected)
    {
        Expected = expected;
    }

    public void Assert(Scenario scenario, AssertionContext context)
    {
        var failure = StatusCodeExpectation.Failure(Expected, context.HttpContext.Response.StatusCode);
        if (failure != null)
        {
            context.AddFailure(failure);

            context.ReadBodyAsString();
        }
    }
}
#endregion
