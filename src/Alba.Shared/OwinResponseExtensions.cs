using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Testing;
using OwinEnvironment = System.Collections.Generic.Dictionary<string, object>;

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
                throw new NotImplementedException();
                //Append(HttpResponseHeaders.ContentLength, fileInfo.Length.ToString(CultureInfo.InvariantCulture));
                using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    //Write(stream => fileStream.CopyTo(stream));
                }
            }



        }

        public static void WriteContentType(this OwinEnvironment env, string contentType)
        {
            throw new NotImplementedException();
            //Append(HttpResponseHeaders.ContentType, contentType);
        }
        /*
        public static long ContentLength
        {
            get
            {
                var headers = env.Get<IDictionary<string, string[]>>(OwinConstants.ResponseHeadersKey);
                return headers.ContainsKey(OwinConstants.ContentLengthHeader)
                    ? long.Parse(headers[OwinConstants.ContentLengthHeader][0])
                    : 0;
            }
            set
            {
                var headers = env.Get<IDictionary<string, string[]>>(OwinConstants.ResponseHeadersKey);
                if (headers.ContainsKey(OwinConstants.ContentLengthHeader))
                {
                    headers[OwinConstants.ContentLengthHeader][0] = value.ToString();
                }
                else
                {
                    headers.Add(OwinConstants.ContentLengthHeader, new[] { value.ToString() });
                }
            }
        }
        */

        public static void Write(this OwinEnvironment env, string content)
        {
            throw new NotImplementedException();
            /*
            var writer = new StreamWriter(_output) { AutoFlush = true };

            writer.Write(content);
            */
        }

        public static void Redirect(this OwinEnvironment env, string url)
        {
            throw new NotImplementedException();
            /*
            if (url.StartsWith("~"))
            {
                url = url.TrimStart('~');
            }

            // TODO: This is a hack, better way to accomplish this?
            env[OwinConstants.ResponseStatusCodeKey] = HttpStatusCode.Redirect;
            Append("Location", url);
            Write(
                string.Format(
                    "<html><head><title>302 Found</title></head><body><h1>Found</h1><p>The document has moved <a href='{0}'>here</a>.</p></body></html>",
                    url));

            */
        }

        public static void WriteResponseCode(this OwinEnvironment env, HttpStatusCode status, string description = null)
        {
            env[OwinConstants.ResponseStatusCodeKey] = status.As<int>();
            env[OwinConstants.ResponseReasonPhraseKey] = description;
        }

        /*
        public int StatusCode
        {
            get
            {
                return env.Get<int>(OwinConstants.ResponseStatusCodeKey);
            }
            set
            {
                env.Set<int>(OwinConstants.ResponseStatusCodeKey, value);
            }
        }
        

        public string StatusDescription
        {
            get
            {
                return env.ContainsKey(OwinConstants.ResponseReasonPhraseKey) ? env.Get<string>(OwinConstants.ResponseReasonPhraseKey) : string.Empty;
            }
            set
            {
                env.Set<string>(OwinConstants.ResponseReasonPhraseKey, value);
            }
        }
        */


        public static void Write(this OwinEnvironment env, Action<Stream> output)
        {
            throw new NotImplementedException("Redo");
            // output(_output);
        }

        // TODO -- need a FlushAsync() for SSE support
        public static void Flush(this OwinEnvironment env)
        {
            throw new NotImplementedException("Redo");
            /*
            if (_output.Length <= 0) return;

            StreamContents(_output);

            _output = new MemoryStream();
            */
        }

        public static void StreamContents(this OwinEnvironment env, MemoryStream recordedStream)
        {
            recordedStream.Position = 0;

            var owinOutput = env.Get<Stream>(OwinConstants.ResponseBodyKey);
            recordedStream.CopyTo(owinOutput);

            recordedStream.Flush();
        }

        public static Stream Output(this OwinEnvironment env)
        {
            throw new NotImplementedException();
        }


        public static HttpResponseBody Body(this OwinEnvironment env)
        {
            return new HttpResponseBody(env.Get<Stream>(OwinConstants.ResponseBodyKey), env);
        }

        /*
        public static IEnumerable<Cookie> Cookies(this OwinEnvironment env)
        {
            return HeaderValueFor(HttpResponseHeaders.SetCookie).Select(CookieParser.ToCookie);
        }

        public Cookie CookieFor(string name)
        {
            return Cookies().FirstOrDefault(x => x.Matches(name));
        }

        public string ContentType()
        {
            return HeaderValueFor(HttpResponseHeaders.ContentType).FirstOrDefault();
        }

        public bool ContentTypeMatches(MimeType mimeType)
        {
            return HeaderValueFor(HttpResponseHeaders.ContentType).Any(x => x.EqualsIgnoreCase(mimeType.Value));
        }
        */
    }
}