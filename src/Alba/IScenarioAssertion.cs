using Microsoft.AspNetCore.Http;
#nullable enable
namespace Alba
{

    // SAMPLE: IScenarioAssertion
    public interface IScenarioAssertion
    {
        void Assert(Scenario scenario, HttpContext context, ScenarioAssertionException ex);
    }
    // ENDSAMPLE
}