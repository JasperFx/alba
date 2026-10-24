using System.Text.Json.Nodes;
using Alba.Assertions;
using Alba.Internal;

namespace Alba;

/// <summary>
/// Expectations for an RFC 9457 problem details response, as written by ASP.NET Core's
/// Results.Problem(), Results.ValidationProblem(), MVC ProblemDetails and Wolverine
/// </summary>
public sealed class ProblemDetailsExpectations
{
    private readonly Scenario _parent;
    private readonly ProblemDetailsAssertion _assertion = new();

    internal ProblemDetailsExpectations(Scenario parent, bool requireErrors)
    {
        _parent = parent;
        parent.AssertThat(_assertion);

        if (requireErrors)
        {
            _assertion.Add(problem => problem["errors"] is JsonObject
                ? null
                : "Expected a validation problem response, but the problem details have no 'errors' member");
        }
    }

    /// <summary>
    /// Asserts the 'type' member of the problem details
    /// </summary>
    public ProblemDetailsExpectations WithType(string type) => member("type", type);

    /// <summary>
    /// Asserts the 'title' member of the problem details
    /// </summary>
    public ProblemDetailsExpectations WithTitle(string title) => member("title", title);

    /// <summary>
    /// Asserts the 'detail' member of the problem details
    /// </summary>
    public ProblemDetailsExpectations WithDetail(string detail) => member("detail", detail);

    /// <summary>
    /// Asserts an extension member of the problem details, comparing it structurally with
    /// the expected value serialized with the application's JSON settings
    /// </summary>
    /// <param name="name"></param>
    /// <param name="expected"></param>
    /// <returns></returns>
    public ProblemDetailsExpectations WithExtension<T>(string name, T expected)
    {
        var expectedJson = _parent.SerializeExpectedJson(expected);

        _assertion.Add(problem => JsonBody.EqualityFailure($"the problem details member '{name}'",
            problem.TryGetPropertyValue(name, out var actual), actual, expectedJson()));

        return this;
    }

    /// <summary>
    /// Asserts that the 'errors' member of a validation problem has an entry for the field.
    /// The field name is matched without regard to case.
    /// </summary>
    /// <param name="field">The field name, such as "Amount"</param>
    /// <param name="messageContaining">When supplied, one of the field's messages must contain this text</param>
    /// <returns></returns>
    public ProblemDetailsExpectations WithValidationError(string field, string? messageContaining = null)
    {
        _assertion.Add(problem =>
        {
            if (problem["errors"] is not JsonObject errors)
            {
                return $"Expected a validation error for '{field}', but the problem details have no 'errors' member";
            }

            // Applications differ in how they case validation error keys
            var entry = errors.FirstOrDefault(x => string.Equals(x.Key, field, StringComparison.OrdinalIgnoreCase));
            if (entry.Key == null)
            {
                var fields = errors.Count == 0 ? "(none)" : string.Join(", ", errors.Select(x => x.Key));
                return $"Expected a validation error for '{field}', but the errors were for: {fields}";
            }

            if (messageContaining == null) return null;

            var messages = entry.Value switch
            {
                JsonArray array => array.Select(x => x?.ToString() ?? "").ToArray(),
                null => [],
                var single => [single.ToString()]
            };

            return messages.Any(x => x.Contains(messageContaining))
                ? null
                : $"Expected a validation error for '{field}' containing '{messageContaining}', but its messages were: {string.Join(", ", messages.Select(x => $"'{x}'"))}";
        });

        return this;
    }

    private ProblemDetailsExpectations member(string name, string expected)
    {
        _assertion.Add(problem => JsonBody.EqualityFailure($"the problem details {name}",
            problem.TryGetPropertyValue(name, out var actual), actual, JsonValue.Create(expected)));

        return this;
    }
}
