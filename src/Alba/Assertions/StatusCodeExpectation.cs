namespace Alba.Assertions;

internal static class StatusCodeExpectation
{
    /// <summary>
    /// The failure message for a status code, or null when it meets the expectation.
    /// A null expectation accepts any 200-299 code
    /// </summary>
    public static string? Failure(int? expected, int actual)
    {
        if (expected.HasValue)
        {
            return actual == expected.Value ? null : $"Expected status code {expected}, but was {actual}";
        }

        return actual is >= 200 and < 300 ? null : $"Expected a status code between 200 and 299, but was {actual}";
    }
}
