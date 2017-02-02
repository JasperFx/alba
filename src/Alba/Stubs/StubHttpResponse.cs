using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Primitives;

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

        public override IResponseCookies Cookies { get; }
        public override bool HasStarted { get; } = true;
    }
}