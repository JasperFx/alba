using System;
using System.Net;
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
            if (context.Response is StubHttpResponse response)
            {
                if (!string.Equals(response.RedirectedTo, Expected, StringComparison.OrdinalIgnoreCase))
                {
                    ex.Add($"Expected to be redirected to '{Expected}' but was '{response.RedirectedTo}'.");
                }

                if (Permanent != response.RedirectedPermanent)
                {
                    ex.Add($"Expected permanent redirect to be '{Permanent}' but it was not.");
                }
            }
            else
            {
                var location = context.Response.Headers["Location"];
                if (!string.Equals(location, Expected, StringComparison.OrdinalIgnoreCase))
                {
                    ex.Add($"Expected to be redirected to '{Expected}' but was '{location}'.");
                }
                
                

                if (Permanent && context.Response.StatusCode != 301)
                {
                    ex.Add($"Expected permanent redirect to be '{Permanent}' but it was not.");
                }
            }
            
            

        }
    }
}
