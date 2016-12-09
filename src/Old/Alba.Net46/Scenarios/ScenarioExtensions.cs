using System;

namespace Alba.Scenarios
{
    public static class ScenarioExtensions
    {
        public static HttpResponseBody Scenario(this IScenarioSupport support, Action<Scenario> configuration)
        {
            var scenario = new Scenario(support);
            configuration(scenario);

            support.Invoke(scenario.Request).Wait();

            return new HttpResponseBody(support, scenario.Request);
        }
    }
}