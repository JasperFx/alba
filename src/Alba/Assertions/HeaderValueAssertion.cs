namespace Alba.Assertions;

internal sealed class HeaderValueAssertion : IScenarioAssertion
{
    private readonly string _headerKey;
    private readonly string _expected;

    public HeaderValueAssertion(string headerKey, string expected)
    {
        _headerKey = headerKey;
        _expected = expected;
    }

    public void Assert(Scenario scenario, AssertionContext context)
    {
        var values = context.HttpContext.Response.Headers[_headerKey];

        switch (values.Count)
        {
            case 0:
                context.AddFailure($"Expected a single header value of '{_headerKey}'='{_expected}', but no values were found on the response");
                break;

            case 1:
                var actual = values.Single();
                if (actual != _expected)
                {
                    context.AddFailure($"Expected a single header value of '{_headerKey}'='{_expected}', but the actual value was '{actual}'");
                }
                break;

            default:
                var valueText = values.Select(x => "'" + x + "'").Aggregate((s1, s2) => $"{s1}, {s2}");
                context.AddFailure($"Expected a single header value of '{_headerKey}'='{_expected}', but the actual values were {valueText}");
                break;
        }
            
    }
}