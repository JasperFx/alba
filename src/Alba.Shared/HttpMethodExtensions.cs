using System.Collections.Generic;
using System.Linq;
using Baseline.Testing;

namespace Alba
{
    public static class HttpMethodExtensions
    {
        public static string HttpMethod(this IDictionary<string, object> env)
        {
            return env.Get<string>(OwinConstants.RequestMethodKey);
        }

        public static IDictionary<string, object> HttpMethod(this IDictionary<string, object> env, string method)
        {
            env.Append(OwinConstants.RequestMethodKey, method);
            return env;
        }

        /// <summary>
        /// The current request matches one of these HTTP methods
        /// </summary>
        /// <param name="request"></param>
        /// <param name="httpMethods"></param>
        /// <returns></returns>
        public static bool HttpMethodMatchesAny(this IDictionary<string, object> request, params string[] httpMethods)
        {
            return httpMethods.Any(x => x.EqualsIgnoreCase(request.HttpMethod()));
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
    }
}