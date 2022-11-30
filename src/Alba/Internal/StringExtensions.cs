using System.Diagnostics.CodeAnalysis;

namespace Alba.Internal;

internal static class StringExtensions
{
    public static bool IsEmpty([NotNullWhen(false)] this string? stringValue) => string.IsNullOrEmpty(stringValue);

    public static bool IsNotEmpty([NotNullWhen(true)] this string? stringValue) => !string.IsNullOrEmpty(stringValue);
}