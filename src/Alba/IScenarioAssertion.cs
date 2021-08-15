using Microsoft.AspNetCore.Http;
#nullable enable
namespace Alba
{

    #region sample_IScenarioAssertion
    public interface IScenarioAssertion
    {
        void Assert(Scenario scenario, HttpContext context, ScenarioAssertionException ex);
    }
    #endregion
}
