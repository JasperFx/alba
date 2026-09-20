using Alba.Internal;

namespace Alba.Assertions;

internal sealed class NoHeaderValueAssertion : IScenarioAssertion
{
    private readonly string _headerKey;

    public NoHeaderValueAssertion(string headerKey)
    {
        _headerKey = headerKey;
    }

    public void Assert(Scenario scenario, AssertionContext context)
    {
        var headers = context.HttpContext.Response.Headers;
        if (headers.TryGetValue(_headerKey, out var values))
        {
            context.AddFailure($"Expected no value for header '{_headerKey}', but found values {values.Quoted()}");
        }
    }
}
