using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Alba
{
    public static class HeaderExtensions
    {
        public static long? GetContentLength(this IHeaderDictionary headers)
        {
            long length;
            var rawValue = headers[HeaderNames.ContentLength];

            if (rawValue.Count == 1 &&
                !string.IsNullOrWhiteSpace(rawValue[0]) &&
                HeaderUtilities.TryParseInt64(rawValue[0], out length))
            {
                return length;
            }

            return null;
        }

        public static void SetContentLength(this IHeaderDictionary headers, long? value)
        {
            if (value.HasValue)
            {
                headers[HeaderNames.ContentLength] = HeaderUtilities.FormatInt64(value.Value);
            }
            else
            {
                headers.Remove(HeaderNames.ContentLength);
            }
        }
    }
}
