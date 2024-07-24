using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
 
namespace Alba;

public static class HeaderExtensions
{
    /// <summary>
    /// Get the content-length header value
    /// </summary>
    /// <param name="headers"></param>
    /// <returns></returns>
    public static long? ContentLength(this IHeaderDictionary headers)
    {
        return headers.ContentLength;
    }

    /// <summary>
    /// Set the content-length header value
    /// </summary>
    /// <param name="headers"></param>
    /// <param name="value"></param>
    public static void ContentLength(this IHeaderDictionary headers, long? value)
    {
        if (value.HasValue)
        {
            headers[HeaderNames.ContentLength] = FormatInt64(value.Value);
        }
        else
        {
            headers.Remove(HeaderNames.ContentLength);
        }
    }

    private static bool TryParseInt64(string input, out long value) {
        return HeaderUtilities.TryParseNonNegativeInt64(input, out value);
    }
    private static string FormatInt64(long input) {
        return HeaderUtilities.FormatNonNegativeInt64(input);
    }
}