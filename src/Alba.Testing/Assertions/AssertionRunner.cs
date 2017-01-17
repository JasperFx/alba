using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.AspNetCore.Http;

namespace Alba.Testing.Assertions
{
    public static class AssertionRunner
    {
        public static ScenarioAssertionException Run(IScenarioAssertion assertion,
            Action<HttpContext> configuration)
        {
            var ex = new ScenarioAssertionException();
            var scenario = new Scenario(new BasicScenarioSupport(), null);
            configuration(scenario.Context);

            var stream = scenario.Context.Response.Body;
            if (stream != null) stream.Position = 0;

            assertion.Assert(scenario, ex);

            return ex;
        }

        public static void SingleMessageShouldBe(this ScenarioAssertionException ex, string message)
        {
            ex.Messages.ShouldHaveTheSameElementsAs(message);
        }
    }
}