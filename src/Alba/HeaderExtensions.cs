using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Alba
{
    public static class HeaderExtensions
    {
        /// <summary>
        /// Get the content-length header value
        /// </summary>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static long? ContentLength(this IHeaderDictionary headers)
        {
            long length;
            var rawValue = headers[HeaderNames.ContentLength];

            if (rawValue.Count == 1 &&
                !string.IsNullOrWhiteSpace(rawValue[0]) &&
                TryParseInt64(rawValue[0], out length))
            {
                return length;
            }

            return null;
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
#if NETSTANDARD2_0
            return HeaderUtilities.TryParseNonNegativeInt64(input, out value);
#else
            return HeaderUtilities.TryParseInt64(input, out value);
#endif
        }
        private static string FormatInt64(long input) {
#if NETSTANDARD2_0
            return HeaderUtilities.FormatNonNegativeInt64(input);
#else
            return HeaderUtilities.FormatInt64(input);
#endif
        }
    }
}
