using System.Collections.Generic;
using System.Linq;
using Baseline;
using HeaderDict = System.Collections.Generic.IDictionary<string, string[]>;

namespace Alba
{
    public static class HeaderDictionaryExtensions
    {
        /// <summary>
        ///     Get the associated values from the collection separated into individual values.
        ///     Quoted values will not be split, and the quotes will be removed.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetDelimitedHeaderValues(this HeaderDict headers, string key)
        {
            return headers.GetAll(key).GetCommaSeparatedHeaderValues();
        }

        /// <summary>
        ///     Gets the first, raw header value for the key
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Get(this HeaderDict headers, string key)
        {
            key = determineKey(headers, key);
            return headers.ContainsKey(key) ? headers[key].FirstOrDefault() : null;
        }

        public static HeaderDict Append(this HeaderDict headers, string key, params string[] values)
        {
            key = determineKey(headers, key);
            if (headers.ContainsKey(key))
            {
                var oldArray = headers[key];
                var newArray = oldArray.Concat(values).ToArray();

                headers[key] = newArray;
            }
            else
            {
                headers[key] = values;
            }

            return headers;
        }


        public static HeaderDict Replace(this HeaderDict headers, string key, string value)
        {
            key = determineKey(headers, key);
            if (headers.ContainsKey(key))
            {
                headers[key] = new[] {value};
            }
            else
            {
                headers.Add(key, new[] {value});
            }

            return headers;
        }


        public static HeaderDict ContentType(this HeaderDict headers, string contentType)
        {
            return headers.Replace("content-type", contentType);
        }

        public static string ContentType(this HeaderDict headers)
        {
            return headers.Get("content-type");
        }

        public static HeaderDict Accepts(this HeaderDict headers, string accepts)
        {
            headers.Replace("accept", accepts);
            return headers;
        }

        public static string Accepts(this HeaderDict headers)
        {
            return headers.Get("accept");
        }

        public static int ContentLength(this HeaderDict headers)
        {
            var raw = headers.Get("content-length");
            return raw.IsEmpty() ? 0 : int.Parse(raw);
        }

        public static HeaderDict ContentLength(this HeaderDict headers, int length)
        {
            headers.Replace("content-length", length.ToString());
            return headers;
        }

        public static bool Has(this HeaderDict headers, string key)
        {
            key = determineKey(headers, key);
            return headers.ContainsKey(key);
        }

        public static IEnumerable<string> GetAll(this HeaderDict headers, string key)
        {
            key = determineKey(headers, key);
            return headers.ContainsKey(key) ? headers[key] : new string[0];
        }

        private static string determineKey(HeaderDict headers, string key)
        {
            return headers.Keys.FirstOrDefault(x => x.EqualsIgnoreCase(key)) ?? key;
        }

    }
}