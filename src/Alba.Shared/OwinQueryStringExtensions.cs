using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace Alba
{
    public static class OwinQueryStringExtensions
    {
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