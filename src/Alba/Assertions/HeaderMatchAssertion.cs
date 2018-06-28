﻿using System.Linq;
using System.Text.RegularExpressions;
using Baseline;

namespace Alba.Assertions
{
    public class HeaderMatchAssertion : IScenarioAssertion
    {
        private readonly string _headerKey;
        private readonly Regex _regex;

        public HeaderMatchAssertion(string headerKey, Regex regex)
        {
            _headerKey = headerKey;
            _regex = regex;
        }

        public void Assert(Scenario scenario, ScenarioAssertionException ex)
        {
            var values = scenario.Context.Response.Headers[_headerKey];

            switch (values.Count)
            {
                case 0:
                    ex.Add($"Expected a single header value of '{_headerKey}' matching '{_regex}', but no values were found on the response");
                    break;

                case 1:
                    var actual = values.Single();
                    if (_regex.IsMatch(actual) == false)
                    {
                        ex.Add($"Expected a single header value of '{_headerKey}' matching '{_regex}', but the actual value was '{actual}'");
                    }
                    break;

                default:
                    var valueText = values.Select(x => "'" + x + "'").Join(", ");
                    ex.Add($"Expected a single header value of '{_headerKey}' matching '{_regex}', but the actual values were {valueText}");
                    break;
            }
        }
    }
}
