using System.Linq;
using Baseline;

namespace Alba.Scenarios
{
    public class NoHeaderValueAssertion : IScenarioAssertion
    {
        private readonly string _headerKey;

        public NoHeaderValueAssertion(string headerKey)
        {
            _headerKey = headerKey;
        }

        public void Assert(Scenario scenario, ScenarioAssertionException ex)
        {
            if (scenario.Request.ResponseHeaders().Has(_headerKey))
            {
                var values = scenario.Request.ResponseHeaders().GetAll(_headerKey);
                var valueText = values.Select(x => "'" + x + "'").Join(", ");
                ex.Add($"Expected no value for header '{_headerKey}', but found values {valueText}");
            }

        }
    }
}