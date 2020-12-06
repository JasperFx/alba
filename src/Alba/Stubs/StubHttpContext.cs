using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;

namespace Alba.Stubs
{
    public class StubHttpContext : HttpContext
    {
        public static StubHttpContext Empty()
        {
            return new(new FeatureCollection(), new ServiceCollection().BuildServiceProvider());
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

        public override WebSocketManager WebSockets => throw new NotSupportedException();


        public override ClaimsPrincipal User { get; set; } = new ClaimsPrincipal();
        public override IDictionary<object, object?> Items { get; set; } = new ItemsDictionary<object, object?>();


        public sealed override IServiceProvider RequestServices { get; set; }


        public override CancellationToken RequestAborted
        {
            get => Cancellation.Token;
            set
            {
                // nothing
            }
        }

        public override string TraceIdentifier { get; set; } = Guid.NewGuid().ToString();


        public override ISession Session { get; set; } = new StubSession();
    }
}