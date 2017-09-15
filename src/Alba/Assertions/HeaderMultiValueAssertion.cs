using System.Collections.Generic;
using System.Linq;
using Baseline;

namespace Alba.Assertions
{
    public class HeaderMultiValueAssertion : IScenarioAssertion
    {
        private readonly string _headerKey;
        private readonly List<string> _expected;

        public HeaderMultiValueAssertion(string headerKey, IEnumerable<string> expected)
        {
            _headerKey = headerKey;
            _expected = expected.ToList();
        }

        public void Assert(Scenario scenario, ScenarioAssertionException ex)
        {
            var values = scenario.Context.Response.Headers[_headerKey];
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
