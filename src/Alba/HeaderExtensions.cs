using Microsoft.AspNetCore.Http;

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
    /// Set the content-length header value, or remove the header for null
    /// </summary>
    /// <param name="headers"></param>
    /// <param name="value"></param>
    public static void ContentLength(this IHeaderDictionary headers, long? value)
    {
        headers.ContentLength = value;
    }
}
