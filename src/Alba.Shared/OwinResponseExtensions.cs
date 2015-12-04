using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Testing;
using OwinEnvironment = System.Collections.Generic.IDictionary<string, object>;

namespace Alba
{
    public static class OwinResponseExtensions
    {
        public static string RequestId(this IDictionary<string, object> http)
        {
            return http.ResponseHeaders().Get(OwinConstants.REQUEST_ID);
        }

        public static void RequestId(this IDictionary<string, object> http, string id)
        {
            http.ResponseHeaders().Replace(OwinConstants.REQUEST_ID, id);
        }

        public static IDictionary<string, string[]> ResponseHeaders(this IDictionary<string, object> dict)
        {
            if (!dict.ContainsKey(OwinConstants.ResponseHeadersKey))
            {
                dict.Add(OwinConstants.ResponseHeadersKey, new Dictionary<string, string[]>());
            }

            return dict[OwinConstants.ResponseHeadersKey].As<IDictionary<string, string[]>>();
        }

        // TODO -- this needs to be tested through integration tests
        // TODO -- add the mimetype support as well
        public static void WriteFile(this OwinEnvironment env, string file)
        {
            var fileInfo = new FileInfo(file);

            if (env.ContainsKey("sendfile.SendAsync"))
            {
                var sendFile = env.Get<Func<string, long, long?, CancellationToken, Task>>("sendfile.SendAsync");
                sendFile(file, 0, fileInfo.Length, env.Get<CancellationToken>(OwinConstants.CallCancelledKey));
            }
            else
            {
                env.ResponseHeaders()
                    .Replace(HttpResponseHeaders.ContentLength, fileInfo.Length.ToString(CultureInfo.InvariantCulture));
                using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    env.Write(stream => fileStream.CopyTo(stream));
                }
            }
        }



        public static void Write(this OwinEnvironment env, string content, string mimeType = null)
        {
            var body = env.ResponseStream();
            var writer = new StreamWriter(body);
            writer.Write(content);
            writer.Flush();

            if (mimeType.IsNotEmpty())
            {
                env.ResponseHeaders().ContentType(mimeType);
            }
        }

        public static void Redirect(this OwinEnvironment env, string url)
        {
            if (url.StartsWith("~"))
            {
                url = url.TrimStart('~');
            }

            env.StatusCode(302);

            env.ResponseHeaders().Replace("Location", url);
            env.Write(
                $"<html><head><title>302 Found</title></head><body><h1>Found</h1><p>The document has moved <a href='{url}'>here</a>.</p></body></html>");

            
        }

        public static OwinEnvironment StatusCode(this OwinEnvironment env, int statusCode,
            string description = null)
        {

            env.Set(OwinConstants.ResponseStatusCodeKey, statusCode);
            if (description.IsNotEmpty())
            {
                env.Set(OwinConstants.ResponseReasonPhraseKey, description);
            }

            return env;
        }

        public static int StatusCode(this OwinEnvironment env)
        {
            return env.Get<int>(OwinConstants.ResponseStatusCodeKey);
        }

        public static string StatusDescription(this OwinEnvironment env)
        {
            return env.Get<string>(OwinConstants.ResponseReasonPhraseKey);
        }

        public static OwinEnvironment StatusDescription(this OwinEnvironment env, string description)
        {
            env.Set(OwinConstants.ResponseReasonPhraseKey, description);
            return env;
        }


        public static void Write(this OwinEnvironment env, Action<Stream> output)
        {
            output(env.ResponseStream());
        }

        public static void Flush(this OwinEnvironment env)
        {
            env.ResponseStream().Flush();
        }

        public static Task FlushAsync(this OwinEnvironment env)
        {
            return env.ResponseStream().FlushAsync();
        }

        public static Task FlushAsync(this OwinEnvironment env, CancellationToken cancellation)
        {
            return env.ResponseStream().FlushAsync(cancellation);
        }

        public static Stream ResponseStream(this OwinEnvironment env)
        {
            if (env.ContainsKey(OwinConstants.ResponseBodyKey))
            {
                return env.Get<Stream>(OwinConstants.ResponseBodyKey);
            }

            var stream = new MemoryStream();
            env.Add(OwinConstants.ResponseBodyKey, stream);

            return stream;
        }


        public static HttpResponseBody Response(this OwinEnvironment env)
        {
            return new HttpResponseBody(env.ResponseStream(), env);
        }


    }
}