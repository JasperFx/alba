using System.Collections.Generic;

namespace Alba.Scenarios
{
    public class SendExpression
    {
        private readonly Dictionary<string, object> _request;

        public SendExpression(Dictionary<string, object> request)
        {
            _request = request;
        }

        public SendExpression ContentType(string contentType)
        {
            _request.RequestHeaders().ContentType(contentType);
            return this;
        }

        public SendExpression Accepts(string accepts)
        {
            _request.RequestHeaders().Accepts(accepts);
            return this;
        }

        public SendExpression Etag(string etag)
        {
            _request.RequestHeaders().Replace(HttpRequestHeaders.IfNoneMatch, etag);
            return this;
        }
    }
}