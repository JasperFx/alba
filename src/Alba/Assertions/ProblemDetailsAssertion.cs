using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;

namespace Alba.Assertions;

internal sealed class ProblemDetailsAssertion : IScenarioAssertion
{
    private readonly List<Func<JsonObject, string?>> _checks = new();

    public void Add(Func<JsonObject, string?> check) => _checks.Add(check);

    public void Assert(Scenario scenario, AssertionContext context)
    {
        var mediaType = context.HttpContext.Response.GetTypedHeaders().ContentType?.MediaType;
        if (mediaType is not { } type || !type.Equals(MimeType.ProblemJson.Value, StringComparison.OrdinalIgnoreCase))
        {
            var contentType = context.HttpContext.Response.ContentType ?? "(none)";
            context.AddFailure(
                $"Expected a problem details response with content type '{MimeType.ProblemJson.Value}', but the content type was '{contentType}'");
        }

        if (!context.TryReadBodyAsJson(out var node) || node is not JsonObject problem)
        {
            context.AddFailure("Expected a problem details response, but the response body is not a JSON object");
            return;
        }

        foreach (var check in _checks)
        {
            var failure = check(problem);
            if (failure != null) context.AddFailure(failure);
        }
    }
}
