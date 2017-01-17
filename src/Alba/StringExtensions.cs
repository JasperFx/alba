using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using Baseline;

namespace Alba
{
    public static class StringExtensions
    {
        public static IEnumerable<string> GetCommaSeparatedHeaderValues(this IEnumerable<string> enumerable)
        {
            foreach (var content in enumerable)
            {
                var searchString = content.Trim();
                if (searchString.Length == 0) break;

                var parser = new CommaTokenParser();
                content.ToCharArray().Each(parser.Read);

                // Gotta force the parser to know it's done
                parser.Read(',');

                foreach (var token in parser.Tokens)
                {
                    yield return token.Trim();
                }
            }


        }

        public static string UrlEncoded(this string value)
        {
            return WebUtility.UrlEncode(value);
        }

        public static string Quoted(this string value)
        {
            return $"\"{value}\"";
        }

        public static DateTime? TryParseHttpDate(this string dateString)
        {
            DateTime date;

            return DateTime.TryParseExact(dateString, "r", CultureInfo.InvariantCulture, DateTimeStyles.None, out date)
                ? date
                : null as DateTime?;
        }

    }
}