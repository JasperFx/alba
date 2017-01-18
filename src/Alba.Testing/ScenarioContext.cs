using System;
using System.Threading.Tasks;

namespace Alba.Testing
{
    public class ScenarioContext
    {
        protected readonly BasicScenarioSupport host = new BasicScenarioSupport();

        protected Task<ScenarioAssertionException> fails(Action<Scenario> configuration)
        {
            return Exception<ScenarioAssertionException>.ShouldBeThrownBy(() => host.Scenario(configuration));
        }
    }
}