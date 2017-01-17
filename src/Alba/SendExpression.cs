using Microsoft.AspNetCore.Http;

namespace Alba
{
    public class SendExpression
    {
        private readonly HttpRequest _request;

        public SendExpression(HttpContext context)
        {
            _request = context.Request;
        }

        public SendExpression ContentType(string contentType)
        {
            _request.Headers["content-type"] = contentType;
            return this;
        }

        public SendExpression Accepts(string accepts)
        {
            _request.Headers["accept"] = accepts;
            return this;
        }

        public SendExpression Etag(string etag)
        {
            _request.Headers["If-None-Match"] = etag;
            return this;
        }
    }
}