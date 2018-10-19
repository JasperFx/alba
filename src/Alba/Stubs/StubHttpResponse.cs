using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Net.Http.Headers;

namespace Alba.Stubs
{
    public class StubHttpResponse : HttpResponse
    {
        private IHttpResponseFeature _feature;

        public StubHttpResponse(StubHttpContext context)
        {
            HttpContext = context;

            _feature = context.Features.Get<IHttpResponseFeature>();

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
            StatusCode = permanent ? 301 : 302;
            
            RedirectedTo = location;
            RedirectedPermanent = permanent;
        }

        public bool RedirectedPermanent { get; set; }

        public string RedirectedTo { get; set; }

        public override HttpContext HttpContext { get; }

        public override int StatusCode
        {
            get => _feature.StatusCode;
            set => _feature.StatusCode = value;
        }
        public override IHeaderDictionary Headers => _feature.Headers;

        public override Stream Body
        {
            get => _feature.Body;
            set => _feature.Body = value;
        } 

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
        public override bool HasStarted => Body.Length > 0;
    }
}