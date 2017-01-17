using System.Linq;
using Baseline;

namespace Alba
{
    public class HasSingleHeaderValueAssertion : IScenarioAssertion
    {
        private readonly string _headerKey;

        public HasSingleHeaderValueAssertion(string headerKey)
        {
            _headerKey = headerKey;
        }

        public void Assert(Scenario scenario, ScenarioAssertionException ex)
        {
            var values = scenario.Context.Response.Headers[_headerKey];

            switch (values.Count)
            {
                case 0:
                    ex.Add(
                        $"Expected a single header value of '{_headerKey}', but no values were found on the response");
                    break;
                case 1:
                    // nothing, thats' good;)
                    break;

                default:
                    var valueText = values.Select(x => "'" + x + "'").Join(", ");
                    ex.Add($"Expected a single header value of '{_headerKey}', but found multiple values on the response: {valueText}");
                    break;
            }
        }
    }
}