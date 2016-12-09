namespace Alba.Scenarios.Assertions
{
    public class BodyDoesNotContainAssertion : IScenarioAssertion
    {
        public string Text { get; set; }

        public BodyDoesNotContainAssertion(string text)
        {
            Text = text;
        }

        public void Assert(Scenario scenario, ScenarioAssertionException ex)
        {
            var body = ex.ReadBody(scenario);
            if (body.Contains(Text))
            {
                ex.Add($"Text '{Text}' should not be found in the response body");
            }
        }
    }
}