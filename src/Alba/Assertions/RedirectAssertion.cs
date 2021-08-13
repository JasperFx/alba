using System;
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
            var location = context.Response.Headers["Location"];
            if (!string.Equals(location, Expected, StringComparison.OrdinalIgnoreCase))
            {
                ex.Add($"Expected to be redirected to '{Expected}' but was '{location}'.");
            }

            new StatusCodeAssertion(Permanent ? 301 : 302).Assert(scenario, context, ex);
        }
    }
}