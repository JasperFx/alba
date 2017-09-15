using System;
using Alba.Stubs;
using Baseline;

namespace Alba.Assertions
{
    public class RedirectAssertion : IScenarioAssertion
    {
        public RedirectAssertion(string expected, bool permanent)
        {
            Expected = expected;
            Permanent = permanent;
        }

        public string Expected { get; }
        public bool Permanent { get; }

        public void Assert(Scenario scenario, ScenarioAssertionException ex)
        {
            var response = scenario.Context.Response.As<StubHttpResponse>();

            if (!string.Equals(response.RedirectedTo, Expected, StringComparison.OrdinalIgnoreCase))
            {
                ex.Add($"Expected to be redirected to '{Expected}' but was '{response.RedirectedTo}'.");
            }

            if (Permanent != response.RedirectedPermanent)
            {
                ex.Add($"Expected permanent redirect to be '{Permanent}' but it was not.");
            }
        }
    }
}
