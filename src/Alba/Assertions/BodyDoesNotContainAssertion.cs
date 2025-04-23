namespace Alba.Assertions;

internal sealed class BodyDoesNotContainAssertion : IScenarioAssertion
{
    public string Text { get; set; }

    public BodyDoesNotContainAssertion(string text)
    {
        Text = text;
    }

    public void Assert(Scenario scenario, AssertionContext context)
    {
        var body = context.ReadBodyAsString();
        if (body.Contains(Text))
        {
            context.AddFailure($"Text '{Text}' should not be found in the response body");
        }
    }
}