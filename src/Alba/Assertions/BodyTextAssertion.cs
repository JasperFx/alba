using Microsoft.AspNetCore.Http;

namespace Alba.Assertions;

internal sealed class BodyTextAssertion : IScenarioAssertion
{
    public string Text { get; set; }

    public BodyTextAssertion(string text)
    {
        Text = text;
    }

    public void Assert(Scenario scenario, HttpContext context, ScenarioAssertionException ex)
    {
        var body = ex.ReadBody(context);
        if (!body.Equals(Text))
        {
            ex.Add($"Expected the content to be '{Text}'");
        }
    }
}