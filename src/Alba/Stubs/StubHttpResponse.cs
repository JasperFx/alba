using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Net.Http.Headers;

namespace Alba.Stubs
{
    public class StubHttpResponse : HttpResponse
    {
        public StubHttpResponse(StubHttpContext context)
        {
            HttpContext = context;

            Cookies = new ResponseCookies(Headers, new DefaultObjectPool<StringBuilder>(new DefaultPooledObjectPolicy<StringBuilder>()));
        }

        public override void OnStarting(Func<object, Task> callback, object state)
        {
        }

        public override void OnCompleted(Func<object, Task> callback, object state)
        {
        }

        public override void Redirect(string location, bool permanent)
        {
            RedirectedTo = location;
            RedirectedPermanent = permanent;
        }

        public bool RedirectedPermanent { get; set; }

        public string RedirectedTo { get; set; }

        public override HttpContext HttpContext { get; }
        public override int StatusCode { get; set; } = 200;
        public override IHeaderDictionary Headers { get; } = new HeaderDictionary();
        public override Stream Body { get; set; } = new MemoryStream();

        public override long? ContentLength
        {
            get => Headers.ContentLength();
            set => Headers.ContentLength(value);
        }

        public override string ContentType
        {
            get => Headers[HeaderNames.ContentType];
            set => Headers[HeaderNames.ContentType] = value;
        }

        public override IResponseCookies Cookies { get; }
        public override bool HasStarted { get; } = true;
    }
}