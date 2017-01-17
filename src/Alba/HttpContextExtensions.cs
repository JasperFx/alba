using System.IO;
using Microsoft.AspNetCore.Http;

namespace Alba
{
    public static class HttpContextExtensions
    {
        public static void ContentType(this HttpRequest request, string mimeType)
        {
            request.Headers["content-type"] = mimeType;
        }

        public static void ContentType(this HttpResponse response, string mimeType)
        {
            response.Headers["content-type"] = mimeType;
        }

        public static void RelativeUrl(this HttpContext context, string relativeUrl)
        {
            context.Request.Path = relativeUrl;
        }

        public static void Accepts(this HttpContext context, string mimeType)
        {
            context.Request.Headers["accept"] = mimeType;
        }

        public static void HttpMethod(this HttpContext context, string method)
        {
            context.Request.Method = method;
        }

        public static void StatusCode(this HttpContext context, int statusCode)
        {
            context.Response.StatusCode = statusCode;
        }

        public static void Write(this HttpResponse response, string content)
        {
            using (var writer = new StreamWriter(response.Body))
            {
                writer.Write(content);
                writer.Flush();
            }
        }
    }
}