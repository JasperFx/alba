using System.Text.Json.Nodes;
using Alba.Assertions;
using Alba.Internal;

namespace Alba;

/// <summary>
/// Expectations for the value at one path in a JSON response body
/// </summary>
public sealed class JsonPathExpectations
{
    private readonly Scenario _parent;
    private readonly JsonPath _path;

    internal JsonPathExpectations(Scenario parent, string path)
    {
        _parent = parent;
        _path = JsonPath.Parse(path);
    }

    /// <summary>
    /// Asserts that the value at the path is structurally equal to the expected value
    /// serialized with the application's JSON settings. Property order and whitespace are
    /// ignored. Use the path "$" to compare the whole response body.
    /// </summary>
    /// <param name="expected">A primitive, anonymous object, collection or DTO</param>
    /// <returns></returns>
    public JsonPathExpectations ShouldBe<T>(T expected)
    {
        var expectedJson = _parent.SerializeExpectedJson(expected);

        return check((found, actual) => JsonBody.EqualityFailure($"JSON at '{_path.Text}'", found, actual, expectedJson()));
    }

    /// <summary>
    /// Asserts that the path exists in the response body. A JSON null value counts as existing.
    /// </summary>
    /// <returns></returns>
    public JsonPathExpectations ShouldExist()
    {
        return check((found, _) => found ? null : $"Expected JSON at '{_path.Text}', but the path was not found");
    }

    /// <summary>
    /// Asserts that the path does not exist in the response body
    /// </summary>
    /// <returns></returns>
    public JsonPathExpectations ShouldNotExist()
    {
        return check((found, actual) =>
            found ? $"Expected no JSON at '{_path.Text}', but it was {JsonBody.Render(actual)}" : null);
    }

    /// <summary>
    /// Asserts that the value at the path is an array with this many items
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    public JsonPathExpectations ShouldHaveCount(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);

        return check((found, actual) => actual switch
        {
            _ when !found => $"Expected an array of {count} items at '{_path.Text}', but the path was not found",
            JsonArray array when array.Count == count => null,
            JsonArray array => $"Expected an array of {count} items at '{_path.Text}', but it had {array.Count}",
            _ => $"Expected an array of {count} items at '{_path.Text}', but it was {JsonBody.Render(actual)}"
        });
    }

    private JsonPathExpectations check(Func<bool, JsonNode?, string?> check)
    {
        _parent.AssertThat(new JsonPathAssertion(_path, check));
        return this;
    }
}
