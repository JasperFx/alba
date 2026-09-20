using Alba.Internal;

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

        if (values.Count == 0)
        {
            context.AddFailure($"Expected header values of '{_headerKey}'={_expected.Quoted()}, but no values were found on the response.");
        }
        else if (!_expected.All(x => values.Contains(x)))
        {
            context.AddFailure($"Expected header values of '{_headerKey}'={_expected.Quoted()}, but the actual values were {values.Quoted()}.");
        }
    }
}
