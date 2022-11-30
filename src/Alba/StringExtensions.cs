using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;

namespace Alba
{
    public static class StringExtensions
    {
        /// <summary>
        /// Helper to expand any comma separated values out into an enumerable of
        /// all the string values
        /// </summary>
        /// <param name="enumerable"></param>
        /// <returns></returns>
        [Obsolete("Use HeaderDictionary.GetCommaSeparatedHeaderValues instead")]
        public static IEnumerable<string> GetCommaSeparatedHeaderValues(this IEnumerable<string> enumerable)
        {
            foreach (var content in enumerable)
            {
                var searchString = content.Trim();
                if (searchString.Length == 0) break;

                var parser = new CommaTokenParser();
                var array = content.ToCharArray();

                foreach (var c in array)
                {
                    parser.Read(c);
                }

                // Gotta force the parser to know it's done
                parser.Read(',');

                foreach (var token in parser.Tokens)
                {
                    yield return token.Trim();
                }
            }


        }

        [Obsolete("Use WebUtility.UrlEncode directly")]
        public static string UrlEncoded(this string value)
        {
            return WebUtility.UrlEncode(value);
        }

        [Obsolete("Copy this extension into your own codebase if you wish to continue using it.")]
        public static string Quoted(this string value)
        {
            return $"\"{value}\"";
        }

        [Obsolete("Copy this extension into your own codebase if you wish to continue using it.")]
        public static DateTime? TryParseHttpDate(this string dateString)
        {
            return DateTime.TryParseExact(dateString, "r", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
                ? date
                : null;
        }

    }
}