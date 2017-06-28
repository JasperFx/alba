using System.Linq;
using System.Reflection;
using Alba.Stubs;
using Baseline;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;

namespace Alba
{
    public class SendExpression
    {
        private readonly StubHttpRequest _request;

        public SendExpression(HttpContext context)
        {
            _request = context.Request.As<StubHttpRequest>();
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

        /// <summary>
        /// Appends a query string paramater to the HttpRequest
        /// </summary>
        /// <param name="paramName"></param>
        /// <param name="paramValue"></param>
        /// <returns></returns>
        public SendExpression QueryString(string paramName, string paramValue)
        {
            _request.AddQueryString(paramName, paramValue);

            return this;
        }

        /// <summary>
        /// Appends query string parameters for the values of all the public 
        /// read/write properties and all public fields of the target object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <returns></returns>
        public SendExpression QueryString<T>(T target)
        {
            typeof(T).GetProperties().Where(x => x.CanRead).Each(prop =>
            {
                var rawValue = prop.GetValue(target, null);
                QueryString(prop.Name, rawValue?.ToString() ?? string.Empty);
            });

            typeof(T).GetFields().Each(field =>
            {
                var rawValue = field.GetValue(target);
                QueryString(field.Name, rawValue?.ToString() ?? string.Empty);
            });


            return this;
        }
    }
}