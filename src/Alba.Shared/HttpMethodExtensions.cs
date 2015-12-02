using System.Collections.Generic;

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
    }
}