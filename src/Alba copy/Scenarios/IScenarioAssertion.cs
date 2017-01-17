namespace Alba.Scenarios
{
    public interface IScenarioAssertion
    {
        void Assert(Scenario scenario, ScenarioAssertionException ex);
    }
}