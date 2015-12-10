using System.Collections.Generic;
using System.IO;
using System.Threading;
using Baseline;

namespace Alba
{
    public static class OwinRequestExtensions
    {
        /// <summary>
        /// Helper function to read the response body as a string with the default content encoding
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string ReadRequestBodyAsText(this IDictionary<string, object> data)
        {
            return data.RequestBody().ReadAllText();
        }

        /// <summary>
        /// Checks whether or not there is any data in the request body
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static bool HasRequestBody(this IDictionary<string, object> data)
        {
            var stream = data.RequestBody();
            return stream != null && stream.CanRead && stream.Length > 0;
        }

   

        public static IDictionary<string, string[]> RequestHeaders(this IDictionary<string, object> env)
        {
            if (!env.ContainsKey(OwinConstants.RequestHeadersKey))
            {
                env.Add(OwinConstants.RequestHeadersKey, new Dictionary<string, string[]>());
            }

            return env.Get<IDictionary<string, string[]>>(OwinConstants.RequestHeadersKey);
        }


        public static Stream RequestBody(this IDictionary<string, object> env)
        {
            return env.Get<Stream>(OwinConstants.RequestBodyKey);
        }

        public static bool IsClientConnected(this IDictionary<string, object> env)
        {
            var cancellation = env.Get<CancellationToken>(OwinConstants.CallCancelledKey);
            return cancellation == null ? false : !cancellation.IsCancellationRequested;
        }
    }
}