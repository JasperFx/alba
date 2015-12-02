using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Alba
{
    public static class OwinRequestExtensions
    {

        public static IDictionary<string, string[]> RequestHeaders(this IDictionary<string, object> env)
        {
            if (!env.ContainsKey(OwinConstants.RequestHeadersKey))
            {
                env.Add(OwinConstants.RequestHeadersKey, new Dictionary<string, string[]>());
            }

            return env.Get<IDictionary<string, string[]>>(OwinConstants.RequestHeadersKey);
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


        /*
        public static HttpRequestBody Body(this OwinEnvironment env)
        {
            return new HttpRequestBody(env);
        }
        */
    }
}