using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Baseline;

namespace Alba
{
    public static class CookieParser
    {
        private static readonly string[] dateFormats = new string[15]
        {
            "ddd, d MMM yyyy H:m:s 'GMT'",
            "ddd, d MMM yyyy H:m:s",
            "d MMM yyyy H:m:s 'GMT'",
            "d MMM yyyy H:m:s",
            "ddd, d MMM yy H:m:s 'GMT'",
            "ddd, d MMM yy H:m:s",
            "d MMM yy H:m:s 'GMT'",
            "d MMM yy H:m:s",
            "dddd, d'-'MMM'-'yy H:m:s 'GMT'",
            "dddd, d'-'MMM'-'yy H:m:s",
            "ddd MMM d H:m:s yyyy",
            "ddd, d MMM yyyy H:m:s zzz",
            "ddd, d MMM yyyy H:m:s",
            "d MMM yyyy H:m:s zzz",
            "d MMM yyyy H:m:s"
        };

        private const string ExpiresToken = "expires";
        private const string MaxAgeToken = "max-age";
        private const string DomainToken = "domain";
        private const string PathToken = "path";
        private const string SecureToken = "secure";
        private const string HttpOnlyToken = "httponly";

        private static readonly Cache<string, Action<Cookie, string>> _setters =
            new Cache<string, Action<Cookie, string>>();


        static CookieParser()
        {
            _setters[ExpiresToken] = (cookie, value) => {
                                                            DateTimeOffset offset;
                                                            if (TryParseDate(value, out offset))
                                                            {
                                                                cookie.Expires = offset;
                                                            }


            };

            _setters[MaxAgeToken] = (cookie, value) => {
                                                           try
                                                           {
                                                               var seconds = int.Parse(value.Trim());
                                                               cookie.MaxAge = seconds.Seconds();
                                                           }
                                                           catch (Exception)
                                                           {

                                                               // doing nothing here
                                                           }
            };

            _setters[DomainToken] = (cookie, value) => { cookie.Domain = value; };

            _setters[PathToken] = (cookie, value) => {
                                                         cookie.Path = value.IsEmpty() ? "/" : value;
            };

            _setters[SecureToken] = (cookie, value) => { cookie.Secure = true; };

            _setters[HttpOnlyToken] = (cookie, value) => { cookie.HttpOnly = true; };

        }

        public static Cookie ToCookie(string headerValue)
        {
            if (headerValue.IsEmpty()) return null;

            var cookie = new Cookie();

            var segments = SplitValues(headerValue).Select(x => new CookieSegment(x.Trim()));
            segments.Each(segment => {
                                         string canonicalKey = segment.Key.ToLowerInvariant();

                                         if (_setters.Has(canonicalKey))
                                         {
                                             _setters[canonicalKey](cookie, segment.Value);
                                         }
                                         else
                                         {
                                             var state = CookieState.For(segment);
                                             cookie.Add(state);
                                         }
            });

            return cookie;
        }

        public static string DateToString(DateTimeOffset dateTime)
        {
            return dateTime.ToUniversalTime().ToString("r", (IFormatProvider)CultureInfo.InvariantCulture);
        }

        public static bool TryParseDate(string input, out DateTimeOffset result)
        {
            return DateTimeOffset.TryParseExact(input, dateFormats, (IFormatProvider)DateTimeFormatInfo.InvariantInfo, DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal, out result);
        }

        public static IEnumerable<string> SplitValues(string input)
        {
            return input.TrimEnd().TrimEnd(';').Split(';').Select(x => x.Trim());
        }
    }
}