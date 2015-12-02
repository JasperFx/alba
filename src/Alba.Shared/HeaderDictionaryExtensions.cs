using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Xml.Serialization;
using Baseline.Testing;
using HeaderDict = System.Collections.Generic.IDictionary<string, string[]>;

namespace Alba
{
    public static class HeaderDictionaryExtensions
    {

        /// <summary>
        /// Get the associated values from the collection separated into individual values.
        /// Quoted values will not be split, and the quotes will be removed.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static IEnumerable<string> GetDelimitedHeaderValues(this HeaderDict headers, string key)
        {
            return headers.GetAll(key).GetCommaSeparatedHeaderValues();
        }

        /// <summary>
        /// Gets the first, raw header value for the key
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
                headers[key] = new[] { value };
            }
            else
            {
                headers.Add(key, new[] { value });
            }

            return headers;
        }


        // TODO -- may move these out to conneg extensions
        public static HeaderDict ContentType(this HeaderDict headers, string contentType)
        {
            return headers.Replace(HttpRequestHeaders.ContentType, contentType);
        }

        public static string ContentType(this HeaderDict headers)
        {
            return headers.Get(HttpRequestHeaders.ContentType);
        }

        public static HeaderDict Accepts(this HeaderDict headers, string accepts)
        {
            headers.Replace(HttpRequestHeaders.Accept, accepts);
            return headers;
        }

        public static string Accepts(this HeaderDict headers)
        {
            return headers.Get(HttpRequestHeaders.Accept);
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


        // TODO -- need a test for this thing
        public static void AppendCookie(this HeaderDict headers, Cookie cookie)
        {

            if (headers.Has(HttpRequestHeaders.Cookie))
            {
                var current = headers.Get(HttpRequestHeaders.Cookie);
                var newValue = $"{current}; {cookie}";

                headers.Replace(HttpRequestHeaders.Cookie, newValue);
            }
            else
            {
                headers.Replace(HttpRequestHeaders.Cookie, cookie.ToString());
            }


        }

    }


    /*
    public static class CurrentHttpRequestExtensions
    {
        public static bool IfUnModifiedSinceHeaderAndModifiedSince(this IDictionary<string, object> request, IFubuFile file)
        {
            var ifUnModifiedSince = request.IfUnModifiedSince();
            return ifUnModifiedSince.HasValue && file.LastModified() > ifUnModifiedSince.Value;
        }

        public static bool IfModifiedSinceHeaderAndNotModified(this IDictionary<string, object> request, IFubuFile file)
        {
            var ifModifiedSince = request.IfModifiedSince();
            return ifModifiedSince.HasValue && file.LastModified().ToUniversalTime() <= ifModifiedSince.Value;
        }

        public static bool IfNoneMatchHeaderMatchesEtag(this IDictionary<string, object> request, IFubuFile file)
        {
            return request.IfNoneMatch().EtagMatches(file.Etag()) == EtagMatch.Yes;
        }

        public static bool IfMatchHeaderDoesNotMatchEtag(this IDictionary<string, object> request, IFubuFile file)
        {
            return request.IfMatch().EtagMatches(file.Etag()) == EtagMatch.No;
        }

    }
    */

        /*
    public class HttpRequestBody
    {
        private readonly Owinheadersironment _parent;

        public HttpRequestBody(Owinheadersironment parent)
        {
            _parent = parent;
        }

        public void XmlInputIs(object target)
        {
            var serializer = new XmlSerializer(target.GetType());
            serializer.Serialize(_parent.Input(), target);
            _parent.Input().Position = 0;
        }

        public void JsonInputIs(object target)
        {
            string json = null;

            if (_parent.ContainsKey("scenario-support"))
            {
                var serializer = _parent["scenario-support"]
                    .As<IScenarioSupport>()
                    .Get<IJsonSerializer>();

                json = serializer.Serialize(target);
            }
            else
            {
                json = JsonUtil.ToJson(target);
            }

            JsonInputIs(json);
        }

        public void JsonInputIs(string json)
        {
            var writer = new StreamWriter(_parent.Input());
            writer.Write(json);
            writer.Flush();

            _parent.Input().Position = 0;
        }

        public void WriteFormData<T>(T target) where T : class
        {
            new TypeDescriptorCache().ForEachProperty(typeof(T), prop =>
            {
                var rawValue = prop.GetValue(target, null);
                var httpValue = rawValue == null ? string.Empty : rawValue.ToString().UrlEncoded();

                _parent.Form()[prop.Name] = httpValue;
            });
        }

        public void ReplaceBody(Stream stream)
        {
            stream.Position = 0;
            _parent.Append(OwinConstants.RequestBodyKey, stream);
        }

    }
    */
}