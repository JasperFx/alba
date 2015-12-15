namespace Alba.Scenarios.Assertions
{
    public class StatusCodeAssertion : IScenarioAssertion
    {
        public int Expected { get; set; }

        public StatusCodeAssertion(int expected)
        {
            Expected = expected;
        }

        public void Assert(Scenario scenario, ScenarioAssertionException ex)
        {
            var statusCode = scenario.Request.StatusCode();
            if (statusCode != Expected)
            {
                ex.Add($"Expected a Status Code of {Expected}, but was {statusCode}");
            }
        }
    }
}