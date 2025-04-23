namespace Alba.Assertions;

internal sealed  class NoHeaderValueAssertion : IScenarioAssertion
{
    private readonly string _headerKey;

    public NoHeaderValueAssertion(string headerKey)
    {
        _headerKey = headerKey;
    }

    public void Assert(Scenario scenario, AssertionContext context)
    {
        var headers = context.HttpContext.Response.Headers;
        if (headers.ContainsKey(_headerKey))
        {
            var values = headers[_headerKey];
            var valueText = values.Select(x => "'" + x + "'").Aggregate((s1, s2) => $"{s1}, {s2}");
            context.AddFailure($"Expected no value for header '{_headerKey}', but found values {valueText}");
        }
    }
}