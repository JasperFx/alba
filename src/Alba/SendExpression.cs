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

        /// <summary>
        /// Set the content-type header value of the outgoing Http request
        /// </summary>
        /// <param name="contentType"></param>
        /// <returns></returns>
        public SendExpression ContentType(string contentType)
        {
            _request.Headers["content-type"] = contentType;
            return this;
        }

        /// <summary>
        /// Set the accepts header value of the outgoing Http request
        /// </summary>
        /// <param name="accepts"></param>
        /// <returns></returns>
        public SendExpression Accepts(string accepts)
        {
            _request.Headers["accept"] = accepts;
            return this;
        }

        /// <summary>
        /// Set the 'if-none-match' header value of the outgoing Http request
        /// </summary>
        /// <param name="etag"></param>
        /// <returns></returns>
        public SendExpression Etag(string etag)
        {
            _request.Headers["If-None-Match"] = etag;
            return this;
        }

        /// <summary>
        /// Designate the relative url for the Http request
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public SendExpression ToUrl(string url)
        {
            _request.Path = url;
            return this;
        }
    }
}