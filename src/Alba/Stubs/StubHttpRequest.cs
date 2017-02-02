using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Primitives;

namespace Alba.Stubs
{
    public class StubHttpRequest : HttpRequest
    {
        public StubHttpRequest(StubHttpContext context)
        {
            HttpContext = context;
        }   

        public override Task<IFormCollection> ReadFormAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public override HttpContext HttpContext { get; }
        public override string Method { get; set; } = "GET";
        public override string Scheme { get; set; } = "http";
        public override bool IsHttps { get; set; } = false;
        public override HostString Host { get; set; } = new HostString("localhost", 5000);
        public override PathString PathBase { get; set; } = "/";
        public override PathString Path { get; set; } = "/";
        public override QueryString QueryString { get; set; } = QueryString.Empty;
        public override IQueryCollection Query { get; set; } = new QueryCollection();
        public override string Protocol { get; set; }
        public override IHeaderDictionary Headers { get; } = new HeaderDictionary();
        public override IRequestCookieCollection Cookies { get; set; } = new RequestCookieCollection();

        public override long? ContentLength
        {
            get
            {
                if (!Headers.ContainsKey("content-length"))
                {
                    return null;
                }

                return long.Parse(Headers["content-length"].First());
            }
            set
            {
                Headers["content-length"] = new StringValues(value.ToString());
            }
        }

        public override string ContentType
        {
            get
            {
                return Headers["content-type"].FirstOrDefault();
            }
            set
            {
                Headers["content-type"] = new StringValues(value);
            }
        }

        public override Stream Body { get; set; } = new MemoryStream();
        public override bool HasFormContentType { get; } = false; // TODO -- do something here.
        public override IFormCollection Form { get; set; }
    }
}