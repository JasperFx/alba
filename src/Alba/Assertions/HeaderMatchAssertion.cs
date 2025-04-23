using System.Text.RegularExpressions;

namespace Alba.Assertions;

internal sealed class HeaderMatchAssertion : IScenarioAssertion
{
    private readonly string _headerKey;
    private readonly Regex _regex;

    public HeaderMatchAssertion(string headerKey, Regex regex)
    {
        _headerKey = headerKey;
        _regex = regex;
    }

    public void Assert(Scenario scenario, AssertionContext context)
    {
        var values = context.HttpContext.Response.Headers[_headerKey];

        switch (values.Count)
        {
            case 0:
                context.AddFailure($"Expected a single header value of '{_headerKey}' matching '{_regex}', but no values were found on the response");
                break;

            case 1:
                var actual = values.Single();
                if (_regex.IsMatch(actual) == false)
                {
                    context.AddFailure($"Expected a single header value of '{_headerKey}' matching '{_regex}', but the actual value was '{actual}'");
                }
                break;

            default:
                var valueText = values.Select(x => "'" + x + "'").Aggregate((s1, s2) => $"{s1}, {s2}");
                context.AddFailure($"Expected a single header value of '{_headerKey}' matching '{_regex}', but the actual values were {valueText}");
                break;
        }
    }
}