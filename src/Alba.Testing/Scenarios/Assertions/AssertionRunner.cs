using System;
using System.Collections.Generic;
using Alba.Scenarios;

namespace Alba.Testing.Scenarios.Assertions
{
    public static class AssertionRunner
    {
        public static ScenarioAssertionException Run(IScenarioAssertion assertion,
            Action<IDictionary<string, object>> configuration)
        {
            var ex = new ScenarioAssertionException();
            var scenario = new Scenario(null);
            configuration(scenario.Request);

            assertion.Assert(scenario, ex);

            return ex;
        }

        public static void SingleMessageShouldBe(this ScenarioAssertionException ex, string message)
        {
            ex.Messages.ShouldHaveTheSameElementsAs(message);
        }
    }
}