using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Alba.Assertions
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
                var valueText = values.Select(x => "'" + x + "'").Aggregate((s1, s2) => $"{s1}, {s2}");
                ex.Add($"Expected no value for header '{_headerKey}', but found values {valueText}");
            }
        }
    }
}