using System;
using System.Collections.Generic;
using System.Linq;
using Baseline.Testing;

namespace Alba
{
    public static class OwinUrlExtensions
    {
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

            if (current.IsEmpty())
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

        public static string RelativeUrlWithoutQueryString(this IDictionary<string, object> env)
        {
            return env.Get<string>(OwinConstants.RequestPathKey);
        }

        public static string RelativeUrl(this IDictionary<string, object> env)
        {
            var url = env.Get<string>(OwinConstants.RequestPathKey);


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

            var pathbase = env.Get<string>(OwinConstants.RequestPathBaseKey);
            if (pathbase.IsEmpty())
            {
                if (url.StartsWith("~/"))
                {
                    return url.TrimStart('~');
                }

                if (!url.StartsWith("/"))
                {
                    return "/" + url;
                }

                return url;
            }

            return $"/{pathbase}/{url.TrimStart('~').TrimStart('/')}";
        }
    }
}