using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Baseline.Testing;

namespace Alba
{
    public static class OwinRequestExtensions
    {


        public static string RawUrl(this IDictionary<string, object> env)
        {
            var queryString = env.Get<string>(OwinConstants.RequestQueryStringKey);
            if (queryString.IsEmpty())
            {
                return env.Get<string>(OwinConstants.RequestPathBaseKey) + env.Get<string>(OwinConstants.RequestPathKey);
            }

            return env.Get<string>("owin.RequestPathBase") + env.Get<string>("owin.RequestPath") + "?" +
                   env.Get<string>("owin.RequestQueryString");
        }

        public static string RelativeUrl(this IDictionary<string, object> env)
        {
            var url = env.Get<string>(OwinConstants.RequestPathKey).TrimStart('/');


            var queryString = env.Get<string>(OwinConstants.RequestQueryStringKey);
            if (queryString.IsNotEmpty())
            {
                url = url + "?" + queryString;
            }

            return url;
        }

        public static IDictionary<string, object> RelativeUrl(this IDictionary<string, object> env, string url)
        {
            var parts = url.Split('?');

            env.Append(OwinConstants.RequestPathKey, parts.First());

            if (url.Contains("?"))
            {
                var querystring = parts.Last();
                if (env.ContainsKey(OwinConstants.RequestQueryStringKey))
                {
                    env[OwinConstants.RequestQueryStringKey] = querystring;
                }
                else
                {
                    env.Add(OwinConstants.RequestQueryStringKey, querystring);
                }
            }

            return env;
        }

        public static IDictionary<string, string[]> RequestHeaders(this IDictionary<string, object> env)
        {
            if (!env.ContainsKey(OwinConstants.RequestHeadersKey))
            {
                env.Add(OwinConstants.RequestHeadersKey, new Dictionary<string, string[]>());
            }

            return env.Get<IDictionary<string, string[]>>(OwinConstants.RequestHeadersKey);
        }



        public static IDictionary<string, object> FullUrl(this IDictionary<string, object> env, string url)
        {
            var parts = url.Split('?');
            var root = parts.First();

            var uri = new Uri(root);
            env.Append(OwinConstants.RequestSchemeKey, uri.Scheme);
            env.Append(OwinConstants.RequestPathBaseKey, string.Empty);
            env.RequestHeaders().AppendHeader(HttpRequestHeaders.Host, uri.Host);
            env.Append(OwinConstants.RequestPathKey, uri.AbsolutePath);

            if (parts.Length == 2)
            {
                env.Append(OwinConstants.RequestQueryStringKey, parts.Last());
            }


            return env;
        }

        public static string FullUrl(this IDictionary<string, object> env)
        {
            var requestPath = env.Get<string>(OwinConstants.RequestPathKey);


            var uriBuilder = env.uriBuilderFor(requestPath);


            var requestQueryString = env.Get<string>(OwinConstants.RequestQueryStringKey);
            if (requestQueryString.IsNotEmpty())
            {
                uriBuilder.Query = requestQueryString;
            }

            return uriBuilder.Uri.ToString();
        }

        private static UriBuilder uriBuilderFor(this IDictionary<string, object> env, string requestPath)
        {
            var requestScheme = env.Get<string>(OwinConstants.RequestSchemeKey);
            var requestPathBase = env.Get<string>(OwinConstants.RequestPathBaseKey);

            // default values, in absence of a host header
            var host = "127.0.0.1";
            var port = string.Equals(requestScheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ? 443 : 80;

            // if a single host header is available
            string[] hostAndPort;
            if (env.RequestHeaders().TryGetValue("Host", out hostAndPort) &&
                hostAndPort != null &&
                hostAndPort.Length == 1 &&
                !String.IsNullOrWhiteSpace(hostAndPort[0]))
            {
                var parts = hostAndPort[0].Split(':');
                host = parts[0];
                if (parts.Length > 1)
                {
                    int.TryParse(parts[1], out port);
                }
            }

            return new UriBuilder(requestScheme, host, port, requestPathBase + requestPath);
        }

        public static string ToFullUrl(this IDictionary<string, object> env, string url)
        {
            if (Uri.IsWellFormedUriString(url, UriKind.Absolute)) return url;

            if (url.StartsWith("~/"))
            {
                // TODO -- need to use the OwinRequestPathBase whatever in this case.  Not really important now, but might 
                // be down the road.
                return url.TrimStart('~');
            }

            if (!url.StartsWith("/"))
            {
                return "/" + url;
            }

            var requestScheme = env.Get<string>(OwinConstants.RequestSchemeKey) + "://";
            if (url.StartsWith(requestScheme, StringComparison.OrdinalIgnoreCase)) return url;

            return env.uriBuilderFor(url).Uri.ToString();
        }

        public static string HttpMethod(this IDictionary<string, object> env)
        {
            return env.Get<string>(OwinConstants.RequestMethodKey);
        }

        public static IDictionary<string, object> HttpMethod(this IDictionary<string, object> env, string method)
        {
            env.Append(OwinConstants.RequestMethodKey, method);
            return env;
        }





        public static NameValueCollection QueryString(this IDictionary<string, object> env)
        {
            throw new NotImplementedException("Do without using HttpUtility");
            /*
            if (!env.ContainsKey(OwinConstants.RequestQueryStringKey)) return new NameValueCollection();

            var values = HttpUtility.ParseQueryString(FubuCore.DictionaryExtensions.Get<string>(env, OwinConstants.RequestQueryStringKey));

            return values;
            */
        }

        public static NameValueCollection Form(this IDictionary<string, object> env)
        {

            if (!env.ContainsKey(OwinConstants.RequestFormKey))
            {
                env.Add(OwinConstants.RequestFormKey, new NameValueCollection());
            }

            return env.Get<NameValueCollection>(OwinConstants.RequestFormKey);
        }


        public static Stream Input(this IDictionary<string, object> env)
        {
            if (!env.ContainsKey(OwinConstants.RequestBodyKey))
            {
                env.Add(OwinConstants.RequestBodyKey, new MemoryStream());
            }

            return env.Get<Stream>(OwinConstants.RequestBodyKey);
        }

        public static bool IsClientConnected(this IDictionary<string, object> env)
        {
            var cancellation = env.Get<CancellationToken>(OwinConstants.CallCancelledKey);
            return cancellation == null ? false : !cancellation.IsCancellationRequested;
        }


        public static void RewindData(this IDictionary<string, object> env)
        {
            if (env.ContainsKey(OwinConstants.RequestFormKey) && env.Form().Count > 0)
            {
                var post = env.formData().Join("&");
                var postBytes = Encoding.Default.GetBytes(post);
                env.Input().Write(postBytes, 0, postBytes.Length);

                env.Remove(OwinConstants.RequestFormKey);
            }

            if (env.ContainsKey(OwinConstants.RequestBodyKey))
            {
                env.Input().Position = 0;
            }
            else
            {
                env.Add(OwinConstants.ResponseBodyKey, new MemoryStream());
            }
        }

        private static IEnumerable<string> formData(this IDictionary<string, object> env)
        {
            throw new NotImplementedException("Do without using HttpUtility");
            /*
            var form = env.Form();
            foreach (var key in form.AllKeys)
            {
                yield return "{0}={1}".ToFormat(key, HttpUtility.HtmlEncode(form[key]));
            }
            */
        }

        /*
        public static HttpRequestBody Body(this OwinEnvironment env)
        {
            return new HttpRequestBody(env);
        }
        */
    }
}