using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Alba.Stubs
{
    public class StubHttpResponse : HttpResponse
    {
        public StubHttpResponse(StubHttpContext context)
        {
            HttpContext = context;
        }

        public override void OnStarting(Func<object, Task> callback, object state)
        {
            throw new NotImplementedException();
        }

        public override void OnCompleted(Func<object, Task> callback, object state)
        {
            throw new NotImplementedException();
        }

        public override void Redirect(string location, bool permanent)
        {
            throw new NotImplementedException();
        }

        public override HttpContext HttpContext { get; }
        public override int StatusCode { get; set; } = 200;
        public override IHeaderDictionary Headers { get; } = new HeaderDictionary();
        public override Stream Body { get; set; } = new MemoryStream();
        public override long? ContentLength { get; set; }
        public override string ContentType { get; set; }
        public override IResponseCookies Cookies { get; }
        public override bool HasStarted { get; }
    }
}