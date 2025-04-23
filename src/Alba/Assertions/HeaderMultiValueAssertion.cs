namespace Alba.Assertions;

internal sealed class HeaderMultiValueAssertion : IScenarioAssertion
{
    private readonly string _headerKey;
    private readonly List<string> _expected;

    public HeaderMultiValueAssertion(string headerKey, IEnumerable<string> expected)
    {
        _headerKey = headerKey;
        _expected = expected.ToList();
    }

    public void Assert(Scenario scenario, AssertionContext context)
    {
        var values = context.HttpContext.Response.Headers[_headerKey];
        var expectedText = _expected.Select(x => "'" + x + "'").Aggregate((s1, s2) => $"{s1}, {s2}");

        switch (values.Count)
        {
            case 0:
                context.AddFailure($"Expected header values of '{_headerKey}'={expectedText}, but no values were found on the response.");
                break;

            default:
                if (!_expected.All(x => values.Contains(x)))
                {
                    var valueText = values.Select(x => "'" + x + "'").Aggregate((s1, s2) => $"{s1}, {s2}");
                    context.AddFailure($"Expected header values of '{_headerKey}'={expectedText}, but the actual values were {valueText}.");
                }
                break;
        }
    }
}