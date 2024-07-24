using Microsoft.AspNetCore.Http;
 
namespace Alba;

#region sample_IScenarioAssertion
public interface IScenarioAssertion
{
    void Assert(Scenario scenario, HttpContext context, ScenarioAssertionException ex);
}
#endregion