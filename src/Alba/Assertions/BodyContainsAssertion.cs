using Microsoft.AspNetCore.Http;

namespace Alba.Assertions;

#region sample_BodyContainsAssertion
internal sealed class BodyContainsAssertion : IScenarioAssertion
{
    public string Text { get; set; }

    public BodyContainsAssertion(string text)
    {
        Text = text;
    }

    public void Assert(Scenario scenario, AssertionContext context)
    {
        var body = context.ReadBodyAsString();
        if (!body.Contains(Text))
        {
            // Add the failure message to the exception. This exception only
            // gets thrown if there are failures.
            context.AddFailure($"Expected text '{Text}' was not found in the response body");
        }
    }
}
#endregion