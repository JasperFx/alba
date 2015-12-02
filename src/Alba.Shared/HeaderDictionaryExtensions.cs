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
            return headers.GetHeader(key).GetCommaSeparatedHeaderValues();
        }

        /// <summary>
        /// Gets the first, raw header value for the key
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetSingleHeader(this HeaderDict headers, string key)
        {
            return headers.GetHeader(key).FirstOrDefault();
        }

        public static HeaderDict AppendHeader(this HeaderDict headers, string key, params string[] values)
        {
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


        public static HeaderDict ReplaceHeader(this HeaderDict headers, string key, string value)
        {
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


        public static HeaderDict ContentType(this HeaderDict env, string contentType)
        {
            return env.ReplaceHeader(HttpRequestHeaders.ContentType, contentType);
        }

        public static HeaderDict Accepts(this HeaderDict headers, string accepts)
        {
            headers.ReplaceHeader(HttpRequestHeaders.Accept, accepts);
            return headers;
        }

        public static bool HasHeader(this HeaderDict env, string key)
        {
            return env.ContainsKey(key) || env.Keys.Any(x => x.EqualsIgnoreCase(key));
        }

        public static IEnumerable<string> GetHeader(this HeaderDict env, string key)
        {
            if (!env.HasHeader(key)) return new string[0];


            key = env.Keys.FirstOrDefault(x => x.EqualsIgnoreCase(key));
            return key.IsEmpty() ? new string[0] : env.Get(key);
        }


        public static void AppendCookie(this HeaderDict headers, Cookie cookie)
        {

            if (headers.ContainsKey(HttpRequestHeaders.Cookie))
            {
                headers[HttpRequestHeaders.Cookie][0] = headers[HttpRequestHeaders.Cookie][0] + "; " +
                                                            cookie.ToString();
            }
            else
            {
                headers.AppendHeader(HttpRequestHeaders.Cookie, cookie.ToString());
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
        private readonly OwinEnvironment _parent;

        public HttpRequestBody(OwinEnvironment parent)
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