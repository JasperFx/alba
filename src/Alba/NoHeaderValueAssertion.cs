using System.Linq;
using Baseline;
using Microsoft.AspNetCore.Http;
 
namespace Alba
{
    internal class NoHeaderValueAssertion : IScenarioAssertion
    {
        private readonly string _headerKey;

        public NoHeaderValueAssertion(string headerKey)
        {
            _headerKey = headerKey;
        }

        public void Assert(Scenario scenario, HttpContext context, ScenarioAssertionException ex)
        {
            var headers = context.Response.Headers;
            if (headers.ContainsKey(_headerKey))
            {
                var values = headers[_headerKey];
                var valueText = values.Select(x => "'" + x + "'").Join(", ");
                ex.Add($"Expected no value for header '{_headerKey}', but found values {valueText}");
            }
        }
    }
}