using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Alba.Assertions
{
    public class StatusCodeSuccessAssertion : IScenarioAssertion
    {
        public void Assert(Scenario scenario, HttpContext context, ScenarioAssertionException ex)
        {
            var statusCode = context.Response.StatusCode;
            if(statusCode < 200 || statusCode >= 300)
            {
                ex.Add($"Expected a status code between 200 and 299, but was {statusCode}");
                ex.ReadBody(context);
            }
        }
    }
}
