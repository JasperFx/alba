namespace Alba
{
    // SAMPLE: IScenarioAssertion
    public interface IScenarioAssertion
    {
        void Assert(Scenario scenario, ScenarioAssertionException ex);
    }
    // ENDSAMPLE
}