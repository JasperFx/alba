using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Alba
{
    public static class OwinQueryStringExtensions
    {
        public static NameValueCollection ParseQueryString(this IDictionary<string, object> env)
        {
            if (!env.ContainsKey(OwinConstants.RequestQueryStringKey)) return new NameValueCollection();

            var values = HttpUtility.ParseQueryString(env.Get<string>(OwinConstants.RequestQueryStringKey));

            return values;
            
        }

        public static string QueryString(this IDictionary<string, object> env)
        {
            return env.Get<string>(OwinConstants.RequestQueryStringKey);
        }

        public static IDictionary<string, object> QueryString(this IDictionary<string, object> env, string querystring)
        {
            env.Set(OwinConstants.RequestQueryStringKey, querystring.TrimStart('?'));

            return env;
        }
    }
}