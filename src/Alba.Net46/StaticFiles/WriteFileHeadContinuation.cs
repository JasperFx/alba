using System.Collections.Generic;
using System.Net;

namespace Alba.StaticFiles
{
    public class WriteFileHeadContinuation : WriterContinuation
    {
        public WriteFileHeadContinuation(IDictionary<string, object> env, IStaticFile file, int status) : base(env, DoNext.Stop)
        {
            File = file;
            Status = status;
        }

        public IStaticFile File { get; }

        public int Status {get;}

        public static void WriteHeaders(IDictionary<string, object> response, IStaticFile file)
        {
            var headers = response.ResponseHeaders();

            var mimeType = MimeType.MimeTypeByFileName(file.Path);
            if (mimeType != null)
            {
                headers.Append(HttpResponseHeaders.ContentType, mimeType.Value);
            }

            headers.Append(HttpGeneralHeaders.LastModified, file.LastModified().ToString("r"));
            headers.Append(HttpResponseHeaders.ETag, file.Etag().Quoted());

        }

        public override void Write(IDictionary<string, object> response)
        {
            WriteHeaders(response, File);

            if (Status == 200)
            {
                response.ResponseHeaders().Append(HttpResponseHeaders.ContentLength, File.Length().ToString());
            }

            response.StatusCode(Status);
        }
    }
}