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
    }
}