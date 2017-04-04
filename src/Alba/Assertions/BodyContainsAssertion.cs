namespace Alba.Assertions
{
    // SAMPLE: BodyContainsAssertion
    public class BodyContainsAssertion : IScenarioAssertion
    {
        public string Text { get; set; }

        public BodyContainsAssertion(string text)
        {
            Text = text;
        }

        public void Assert(Scenario scenario, ScenarioAssertionException ex)
        {
            var body = ex.ReadBody(scenario);
            if (!body.Contains(Text))
            {
                // Add the failure message to the exception. This exception only
                // gets thrown if there are failures.
                ex.Add($"Expected text '{Text}' was not found in the response body");
            }
        }
    }
    // ENDSAMPLE
}