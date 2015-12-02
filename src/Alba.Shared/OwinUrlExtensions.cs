using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Baseline.Testing;

namespace Alba
{
    public static class OwinUrlExtensions
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

        public static IDictionary<string, object> FullUrl(this IDictionary<string, object> env, string url)
        {
            var parts = url.Split('?');
            var root = parts.First();

            var uri = new Uri(root);
            env.Append(OwinConstants.RequestSchemeKey, uri.Scheme);
            env.Append(OwinConstants.RequestPathBaseKey, String.Empty);
            env.RequestHeaders().Append(HttpRequestHeaders.Host, uri.Host);
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
            var port = String.Equals(requestScheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase) ? 443 : 80;

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
                    Int32.TryParse(parts[1], out port);
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

        public static NameValueCollection QueryString(this IDictionary<string, object> env)
        {
            throw new NotImplementedException("Do without using HttpUtility");
            /*
            if (!env.ContainsKey(OwinConstants.RequestQueryStringKey)) return new NameValueCollection();

            var values = HttpUtility.ParseQueryString(FubuCore.DictionaryExtensions.Get<string>(env, OwinConstants.RequestQueryStringKey));

            return values;
            */
        }

        public static IDictionary<string, object> QueryString(this IDictionary<string, object> env, string querystring)
        {
            throw new NotImplementedException();
        } 
    }
}