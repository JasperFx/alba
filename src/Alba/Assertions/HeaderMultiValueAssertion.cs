using System.Collections.Generic;
using System.Linq;
using Baseline;
using Microsoft.AspNetCore.Http;

namespace Alba.Assertions
{
    internal class HeaderMultiValueAssertion : IScenarioAssertion
    {
        private readonly string _headerKey;
        private readonly List<string> _expected;

        public HeaderMultiValueAssertion(string headerKey, IEnumerable<string> expected)
        {
            _headerKey = headerKey;
            _expected = expected.ToList();
        }

        public void Assert(Scenario scenario, HttpContext context, ScenarioAssertionException ex)
        {
            var values = context.Response.Headers[_headerKey];
            var expectedText = _expected.Select(x => "'" + x + "'").Join(", ");

            switch (values.Count)
            {
                case 0:
                    ex.Add($"Expected header values of '{_headerKey}'={expectedText}, but no values were found on the response.");
                    break;

                default:
                    if (!_expected.All(x => values.Contains(x)))
                    {
                        var valueText = values.Select(x => "'" + x + "'").Join(", ");
                        ex.Add($"Expected header values of '{_headerKey}'={expectedText}, but the actual values were {valueText}.");
                    }
                    break;
            }
        }
    }
}
