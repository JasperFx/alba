namespace Alba.Scenarios.Assertions
{
    public class BodyTextAssertion : IScenarioAssertion
    {
        public string Text { get; set; }

        public BodyTextAssertion(string text)
        {
            Text = text;
        }

        public void Assert(Scenario scenario, ScenarioAssertionException ex)
        {
            var body = ex.ReadBody(scenario);
            if (!body.Equals(Text))
            {
                ex.Add($"The contents should have been:\n\nText");
            }
        }
    }
}