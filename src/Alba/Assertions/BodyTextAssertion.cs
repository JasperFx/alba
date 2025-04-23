namespace Alba.Assertions;

internal sealed class BodyTextAssertion : IScenarioAssertion
{
    public string Text { get; set; }

    public BodyTextAssertion(string text)
    {
        Text = text;
    }

    public void Assert(Scenario scenario, AssertionContext context)
    {
        var body = context.ReadBodyAsString();
        if (!body.Equals(Text))
        {
            context.AddFailure($"Expected the content to be '{Text}'");
        }
    }
}