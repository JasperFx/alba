using System.Text.Json.Nodes;
using Alba.Internal;

namespace Alba.Assertions;

internal sealed class JsonPathAssertion : IScenarioAssertion
{
    private readonly JsonPath _path;
    private readonly Func<bool, JsonNode?, string?> _check;

    public JsonPathAssertion(JsonPath path, Func<bool, JsonNode?, string?> check)
    {
        _path = path;
        _check = check;
    }

    public void Assert(Scenario scenario, AssertionContext context)
    {
        if (!context.TryReadBodyAsJson(out var root))
        {
            context.AddFailure($"Expected JSON at '{_path.Text}', but the response body is not valid JSON");
            return;
        }

        var found = _path.TryFind(root, out var node);
        var failure = _check(found, node);
        if (failure != null) context.AddFailure(failure);
    }
}
