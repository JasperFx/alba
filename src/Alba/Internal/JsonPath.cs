using System.Globalization;
using System.Text.Json.Nodes;

namespace Alba.Internal;

/// <summary>
/// The simple JSONPath subset used by JSON assertions: the root "$", ".name",
/// "['name']" and "[index]" segments
/// </summary>
internal sealed class JsonPath
{
    private readonly object[] _segments;

    private JsonPath(string text, object[] segments)
    {
        Text = text;
        _segments = segments;
    }

    public string Text { get; }

    public static JsonPath Parse(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        var segments = new List<object>();
        var text = path.Trim();
        var i = text.StartsWith('$') ? 1 : 0;

        // "name" and "items[0]" are shorthand for "$.name" and "$.items[0]"
        if (i == 0 && text[0] != '.' && text[0] != '[') text = "." + text;

        while (i < text.Length)
        {
            if (text[i] == '.')
            {
                var start = ++i;
                while (i < text.Length && text[i] != '.' && text[i] != '[') i++;
                if (i == start) throw invalid(path, "a property name is missing after '.'");
                segments.Add(text[start..i]);
            }
            else if (text[i] == '[')
            {
                var close = text.IndexOf(']', i);
                if (close < 0) throw invalid(path, "a '[' is not closed");

                var inner = text[(i + 1)..close].Trim();
                if (inner.Length >= 2 && (inner[0] == '\'' || inner[0] == '"') && inner[^1] == inner[0])
                {
                    segments.Add(inner[1..^1]);
                }
                else if (int.TryParse(inner, NumberStyles.None, CultureInfo.InvariantCulture, out var index))
                {
                    segments.Add(index);
                }
                else
                {
                    throw invalid(path, $"'[{inner}]' is not a quoted property name or an array index");
                }

                i = close + 1;
            }
            else
            {
                throw invalid(path, $"unexpected '{text[i]}' at position {i}");
            }
        }

        return new JsonPath(path, segments.ToArray());
    }

    private static ArgumentException invalid(string path, string reason)
    {
        return new ArgumentException(
            $"'{path}' is not a supported JSON path: {reason}. Use '$', '.name', \"['name']\" and '[index]' segments.",
            nameof(path));
    }

    /// <summary>
    /// Finds the node at this path. A found node can still be null for a JSON null value.
    /// </summary>
    public bool TryFind(JsonNode? root, out JsonNode? node)
    {
        node = root;
        foreach (var segment in _segments)
        {
            switch (segment)
            {
                case string name when node is JsonObject obj && obj.TryGetPropertyValue(name, out var child):
                    node = child;
                    break;

                case int index when node is JsonArray array && index < array.Count:
                    node = array[index];
                    break;

                default:
                    node = null;
                    return false;
            }
        }

        return true;
    }
}
