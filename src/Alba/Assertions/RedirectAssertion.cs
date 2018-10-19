using System;
using Alba.Stubs;
using Baseline;
using Microsoft.AspNetCore.Http;

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

        public void Assert(Scenario scenario, HttpContext context, ScenarioAssertionException ex)
        {
            var response = context.Response.As<StubHttpResponse>();

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
