namespace Alba
{
    public interface IScenarioAssertion
    {
        void Assert(Scenario scenario, ScenarioAssertionException ex);
    }
}