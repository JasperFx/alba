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
        // Context has this useful extension to read the body as a string.
        // This will bake the body contents into the exception message to make debugging easier.
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