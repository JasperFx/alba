using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Alba.Internal;

internal static class JsonBody
{
    private static readonly JsonSerializerOptions Readable = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public static bool TryParse(string body, out JsonNode? node)
    {
        try
        {
            node = JsonNode.Parse(body);
            return true;
        }
        catch (JsonException)
        {
            node = null;
            return false;
        }
    }

    /// <summary>
    /// Renders the node as compact JSON for assertion messages
    /// </summary>
    public static string Render(JsonNode? node) => node?.ToJsonString(Readable) ?? "null";

    /// <summary>
    /// The failure message when the actual value is missing or not structurally equal to the
    /// expected value, or null when they match. The subject names what is compared, such as
    /// "JSON at '$.id'".
    /// </summary>
    public static string? EqualityFailure(string subject, bool found, JsonNode? actual, JsonNode? expected)
    {
        if (!found) return $"Expected {subject} to be {Render(expected)}, but it was not found";

        return JsonNode.DeepEquals(actual, expected)
            ? null
            : $"Expected {subject} to be {Render(expected)}, but it was {Render(actual)}";
    }
}
