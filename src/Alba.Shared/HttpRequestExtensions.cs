using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Baseline.Testing;

namespace Alba
{
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// The current request matches one of these HTTP methods
        /// </summary>
        /// <param name="request"></param>
        /// <param name="httpMethods"></param>
        /// <returns></returns>
        public static bool HttpMethodMatchesAny(this IDictionary<string, object> request, params string[] httpMethods)
        {
            return httpMethods.Any(x => StringExtensions.EqualsIgnoreCase(x, request.HttpMethod()));
        }

        /// <summary>
        /// Evaluates if the current request is for an HTTP method *other* than the supplied httpMethods
        /// </summary>
        /// <param name="request"></param>
        /// <param name="httpMethods"></param>
        /// <returns></returns>
        public static bool IsNotHttpMethod(this IDictionary<string, object> request, params string[] httpMethods)
        {
            return !request.HttpMethodMatchesAny(httpMethods);
        }

        /// <summary>
        /// Is the current request an Http GET?
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool IsGet(this IDictionary<string, object> request)
        {
            return request.HttpMethod().EqualsIgnoreCase("GET");
        }

        /// <summary>
        /// Is the current request an Http POST?
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool IsPost(this IDictionary<string, object> request)
        {
            return request.HttpMethod().EqualsIgnoreCase("POST");
        }

        /// <summary>
        /// Is the current request an Http HEAD?
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool IsHead(this IDictionary<string, object> request)
        {
            return request.HttpMethod().EqualsIgnoreCase("HEAD");
        }

        /// <summary>
        /// Is the currrent request an Http PUT?
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static bool IsPut(this IDictionary<string, object> request)
        {
            return request.HttpMethod().EqualsIgnoreCase("PUT");
        }

        /// <summary>
        /// Converts the given url to a url relative to the current request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static string ToRelativeContentUrl(this IDictionary<string, object> request, string url)
        {
            var current = request.RelativeUrl().TrimStart('/');
            var contentUrl = url.TrimStart('/');

            if (current == string.Empty)
            {
                return contentUrl;
            }

            if (contentUrl.StartsWith(current))
            {
                return contentUrl.Substring(current.Length).TrimStart('/');
            }

            var prepend = current.Split('/').Select(x => "..").Join("/");
            var relativeUrl = prepend.AppendUrl(contentUrl);

            return relativeUrl;
        }


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

        public static string Quoted(this string value)
        {
            return "\"{0}\"".ToFormat(value);
        }





        public static DateTime? TryParseHttpDate(this string dateString)
        {
            DateTime date;

            return DateTime.TryParseExact(dateString, "r", CultureInfo.InvariantCulture, DateTimeStyles.None, out date)
                ? date
                : null as DateTime?;
        }

        public static string ToHttpDateString(this DateTime time)
        {
            return time.ToUniversalTime().ToString("r");
        }

        public static DateTime? IfModifiedSince(this IDictionary<string, object> request)
        {
            return request.RequestHeaders().Get(HttpRequestHeaders.IfModifiedSince)
                .TryParseHttpDate();
        }

        public static DateTime? IfUnModifiedSince(this IDictionary<string, object> request)
        {
            return request.RequestHeaders().Get(HttpRequestHeaders.IfUnmodifiedSince)
                .TryParseHttpDate();
        }

        public static IEnumerable<string> IfMatch(this IDictionary<string, object> request)
        {
            return request.RequestHeaders().GetAll(HttpRequestHeaders.IfMatch).GetCommaSeparatedHeaderValues();
        }

        public static IEnumerable<string> IfNoneMatch(this IDictionary<string, object> request)
        {
            return request.RequestHeaders().GetAll(HttpRequestHeaders.IfNoneMatch).GetCommaSeparatedHeaderValues();
        }



        public static IDictionary<string, object> IfNoneMatch(this IDictionary<string, object> env, string etag)
        {
            env.RequestHeaders().Replace(HttpRequestHeaders.IfNoneMatch, etag);
            return env;
        }

        public static IDictionary<string, object> IfMatch(this IDictionary<string, object> env, string etag)
        {
            env.RequestHeaders().Replace(HttpRequestHeaders.IfMatch, etag);
            return env;
        }

        public static IDictionary<string, object> IfModifiedSince(this IDictionary<string, object> env, DateTime time)
        {
            env.RequestHeaders().Append(HttpRequestHeaders.IfModifiedSince, time.ToUniversalTime().ToString("r"));
            return env;
        }

        public static IDictionary<string, object> IfUnModifiedSince(this IDictionary<string, object> env, DateTime time)
        {
            env.RequestHeaders().Append(HttpRequestHeaders.IfUnmodifiedSince, time.ToUniversalTime().ToString("r"));
            return env;
        }

        public static EtagMatch EtagMatches(this IEnumerable<string> values, string etag)
        {
            if (values == null || !values.Any()) return EtagMatch.None;

            return values.Any(x => x.Equals(etag, StringComparison.Ordinal) || x == "*")
                ? EtagMatch.Yes
                : EtagMatch.No;

        }

        /// <summary>
        /// Helper function to read the response body as a string with the default content encoding
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string InputText(this IDictionary<string, object> data)
        {
            var reader = new StreamReader(data.Input());
            return reader.ReadToEnd();
        }

        /// <summary>
        /// Checks whether or not there is any data in the request body
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool HasBodyData(this IDictionary<string, object> data)
        {
            var stream = data.Input();
            return stream != null && stream.CanRead && stream.Length > 0;
        }

        public static bool CouldBeJson(this IDictionary<string, object> data)
        {
            if (!data.HasBodyData()) return false;

            var stream = data.Input();
            var reader = new StreamReader(stream);
            var firstCharacter = reader.Read();
            stream.Position = 0;

            return firstCharacter == '{';
        }

        public static readonly string LOG_KEY = "fubu.ExecutionLog";
        public static readonly string REQUEST_ID = "x-request-id";

        public static void Set<T>(this IDictionary<string, object> dict, string key, T value)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] = value;
            }
            else
            {
                dict.Add(key, value);
            }
        }

        public static string RequestId(this IDictionary<string, object> http)
        {
            var raw = http.ResponseHeaders();

            if (raw == null) return null;

            if (raw.ContainsKey(REQUEST_ID))
            {
                return raw[REQUEST_ID].FirstOrDefault();
            }

            return null;

        }

        public static void RequestId(this IDictionary<string, object> http, string id)
        {
            http.ResponseHeaders().Append(REQUEST_ID, id);
        }

        /*
        public static string RequestId(this IHttpResponse response)
        {
            var raw = response.HeaderValueFor(REQUEST_ID);
            if (raw == null || !raw.Any()) return null;

            return raw.First();
        }
        */

        public static void CopyTo(this IDictionary<string, object> source, IDictionary<string, object> destination,
            params string[] keys)
        {
            keys.Where(source.ContainsKey).Each(x => destination.Add(x, source[x]));
        }

        public static IDictionary<string, string[]> ResponseHeaders(this IDictionary<string, object> dict)
        {
            if (!dict.ContainsKey(OwinConstants.ResponseHeadersKey))
            {
                dict.Add(OwinConstants.ResponseHeadersKey, new Dictionary<string, string[]>());
            }

            return dict[OwinConstants.ResponseHeadersKey].As<IDictionary<string, string[]>>();
        }



    }
}