namespace Alba.Internal;

internal static class HeaderValueText
{
    /// <summary>
    /// Formats header values for assertion messages as 'a', 'b', 'c'
    /// </summary>
    public static string Quoted(this IEnumerable<string?> values)
    {
        return string.Join(", ", values.Select(x => $"'{x}'"));
    }
}
