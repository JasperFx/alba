using System.Linq;
using Baseline;

namespace Alba
{
    public class HeaderValueAssertion : IScenarioAssertion
    {
        private readonly string _headerKey;
        private readonly string _expected;

        public HeaderValueAssertion(string headerKey, string expected)
        {
            _headerKey = headerKey;
            _expected = expected;
        }

        public void Assert(Scenario scenario, ScenarioAssertionException ex)
        {
            var values = scenario.Context.Response.Headers[_headerKey];

            switch (values.Count)
            {
                case 0:
                    ex.Add($"Expected a single header value of '{_headerKey}'='{_expected}', but no values were found on the response");
                    break;

                case 1:
                    var actual = values.Single();
                    if (actual != _expected)
                    {
                        ex.Add($"Expected a single header value of '{_headerKey}'='{_expected}', but the actual value was '{actual}'");
                    }
                    break;

                default:
                    var valueText = values.Select(x => "'" + x + "'").Join(", ");
                    ex.Add($"Expected a single header value of '{_headerKey}'='{_expected}', but the actual values were {valueText}");
                    break;
            }
            
        }
    }
}