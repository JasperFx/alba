namespace Alba;

#region sample_IScenarioAssertion
public interface IScenarioAssertion
{
    void Assert(Scenario scenario, AssertionContext context);
}
#endregion