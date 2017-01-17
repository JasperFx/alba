using System.Linq;
using Baseline;

namespace Alba
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
            if (scenario.Context.Request.Headers.ContainsKey(_headerKey))
            {
                var values = scenario.Context.Request.Headers[_headerKey];
                var valueText = values.Select(x => "'" + x + "'").Join(", ");
                ex.Add($"Expected no value for header '{_headerKey}', but found values {valueText}");
            }

        }
    }
}