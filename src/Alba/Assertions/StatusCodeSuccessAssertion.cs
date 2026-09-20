namespace Alba.Assertions;

public sealed class StatusCodeSuccessAssertion : IScenarioAssertion
{
    public void Assert(Scenario scenario, AssertionContext context)
    {
        var failure = StatusCodeExpectation.Failure(null, context.HttpContext.Response.StatusCode);
        if (failure != null)
        {
            context.AddFailure(failure);
            context.ReadBodyAsString();
        }
    }
}
