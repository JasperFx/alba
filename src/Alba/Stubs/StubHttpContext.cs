using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;

#if !NETCOREAPP3_0
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http.Internal;
#endif

namespace Alba.Stubs
{
    public class StubHttpContext : HttpContext
    {
        public static StubHttpContext Empty()
        {
            return new StubHttpContext(new FeatureCollection(), null);
        }

        public StubHttpContext(IFeatureCollection features, IServiceProvider services)
        {
            Features = features;

            features.Set<IHttpResponseFeature>(new HttpResponseFeature
            {
                Body = new MemoryStream()
            });

            // Watch this. What is this?
            RequestServices = services;

            Request = new StubHttpRequest(this);
            Response = new StubHttpResponse(this);

            Cancellation = new CancellationTokenSource();

#if !NETCOREAPP3_0
            Authentication = new StubAuthenticationManager(this);
#endif
        }

        public CancellationTokenSource Cancellation { get; }

        public override void Abort()
        {
            Cancellation.Cancel();
        }

        public override IFeatureCollection Features { get; }
        public override HttpRequest Request { get; }
        public override HttpResponse Response { get; }


        public override ConnectionInfo Connection { get; } = new StubConnectionInfo();

        public override WebSocketManager WebSockets
        {
            get { throw new NotSupportedException(); }
        }


#if !NETCOREAPP3_0
        // TODO -- need to see how this puppy is used
        [Obsolete("This is obsolete and will be removed in a future version. The recommended alternative is to use Microsoft.AspNetCore.Authentication.AuthenticationHttpContextExtensions. See https://go.microsoft.com/fwlink/?linkid=845470.")]
        public override AuthenticationManager Authentication { get; }
#endif

        public override ClaimsPrincipal User { get; set; } = new ClaimsPrincipal();


#if !NETCOREAPP3_0
        public override IDictionary<object, object> Items { get; set; } = new ItemsDictionary();
#else
        public override IDictionary<object, object> Items { get; set; } = new ItemsDictionary<object, object>();
#endif
        

        public sealed override IServiceProvider RequestServices { get; set; }


        public override CancellationToken RequestAborted
        {
            get { return Cancellation.Token; }
            set
            {
                // nothing
            }
        }

        public override string TraceIdentifier { get; set; } = Guid.NewGuid().ToString();


        public override ISession Session { get; set; } = new StubSession();
    }
}