using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Net.Http.Headers;

#if !NETCOREAPP3_0
using Microsoft.AspNetCore.Http.Internal;
#endif

namespace Alba.Stubs
{
    public class StubHttpResponse : HttpResponse
    {
        private IHttpResponseFeature _httpResponseFeature;
#if NETCOREAPP3_0
        private IFeatureCollection _features;
#endif

        public StubHttpResponse(StubHttpContext context)
        {
            HttpContext = context;

            _httpResponseFeature = context.Features.Get<IHttpResponseFeature>();
            
#if NETCOREAPP3_0
            Cookies = new StubResponseCookieCollection();
            _features = context.Features;
            _features.Set<IHttpResponseBodyFeature>(new StreamResponseBodyFeature(new MemoryStream()));
#else
            Cookies =
 new ResponseCookies(Headers, new DefaultObjectPool<StringBuilder>(new DefaultPooledObjectPolicy<StringBuilder>()));
#endif
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
            get => _httpResponseFeature.StatusCode;
            set => _httpResponseFeature.StatusCode = value;
        }
        public override IHeaderDictionary Headers => _httpResponseFeature.Headers;

        
#if !NETCOREAPP3_0
        public override Stream Body
        {
            get => _httpResponseFeature.Body;
            set => _httpResponseFeature.Body = value;
        } 
#else
        public override Stream Body
        {
            get => _features.Get<IHttpResponseBodyFeature>().Stream;
            set
            {
                // This is lifted from DefaultHttpResponse
                IHttpResponseBodyFeature priorFeature = _features.Get<IHttpResponseBodyFeature>();
                if (priorFeature is StreamResponseBodyFeature streamResponseBodyFeature 
                    && streamResponseBodyFeature.PriorFeature != null 
                    && value == streamResponseBodyFeature.PriorFeature.Stream)
                {
                  _features.Set(streamResponseBodyFeature.PriorFeature);
                  streamResponseBodyFeature.Dispose();
                }
                else
                {
                  StreamResponseBodyFeature responseBodyFeature2 = new StreamResponseBodyFeature(value, priorFeature);
                  OnCompleted(responseBodyFeature2.CompleteAsync);
                  _features.Set<IHttpResponseBodyFeature>(responseBodyFeature2);
                }
            }
        }
#endif

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