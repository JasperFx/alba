namespace Alba.Assertions;

internal sealed class HasSingleHeaderValueAssertion : IScenarioAssertion
{
    private readonly string _headerKey;

    public HasSingleHeaderValueAssertion(string headerKey)
    {
        _headerKey = headerKey;
    }

    public void Assert(Scenario scenario, AssertionContext context)
    {
        var values = context.HttpContext.Response.Headers[_headerKey];

        switch (values.Count)
        {
            case 0:
                context.AddFailure(
                    $"Expected a single header value of '{_headerKey}', but no values were found on the response");
                break;
            case 1:
                // nothing, thats' good;)
                break;

            default:
                var valueText = values.Select(x => "'" + x + "'").Aggregate((s1, s2) => $"{s1}, {s2}");
                context.AddFailure($"Expected a single header value of '{_headerKey}', but found multiple values on the response: {valueText}");
                break;
        }
    }
}