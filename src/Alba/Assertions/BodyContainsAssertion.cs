namespace Alba.Assertions
{
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
                ex.Add($"Expected text '{Text}' was not found in the response body");
            }
        }
    }
}