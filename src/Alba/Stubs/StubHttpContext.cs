using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Http.Features;

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

            // Watch this. What is this?
            RequestServices = services;

            Request = new StubHttpRequest(this);
            Response = new StubHttpResponse(this);

            Cancellation = new CancellationTokenSource();

            Authentication = new StubAuthenticationManager(this);
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
            get
            {
                throw new NotSupportedException();
            }
        }


        // TODO -- need to see how this puppy is used
        public override AuthenticationManager Authentication { get; }
        public override ClaimsPrincipal User { get; set; } = new ClaimsPrincipal();


        public override IDictionary<object, object> Items { get; set; } = new ItemsDictionary();

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