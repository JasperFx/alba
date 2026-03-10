namespace Alba.Assertions;

public sealed class StatusCodeSuccessAssertion : IScenarioAssertion, IStatusCodeAssertion
{
    public void Assert(Scenario scenario, AssertionContext context)
    {
        var statusCode = context.HttpContext.Response.StatusCode;
       
        if(statusCode < 200 || statusCode >= 300)
        {
            context.AddFailure($"Expected a status code between 200 and 299, but was {statusCode}");
            context.ReadBodyAsString();
        }
    }
}