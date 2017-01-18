namespace Alba.Assertions
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
                ex.Add($"Expected the content to be '{Text}'");
            }
        }
    }
}