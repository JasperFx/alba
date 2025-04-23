using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Alba.Internal;

internal static class AlbaTracing
{
    private static readonly ActivitySource Source = new("Alba");
    
    public const string HttpUrl = "http.url";
    public const string HttpMethod = "http.method";
    public const string HttpRequestContentLength = "http.request_content_length";
    public const string NetPeerName = "net.peer.name";

    public const string HttpStatusCode = "http.status_code";
    public const string HttpResponseContentLength = "http.response_content_length";
    
    public static Activity? StartRequestActivity(HttpRequest request)
    {
        var activity = Source.StartActivity($"{request.Method} {request.Path}", ActivityKind.Client);
        activity?.SetRequestTags(request);
        return activity;
    }

    public static void SetRequestTags(this Activity activity, HttpRequest request)
    {
        activity.SetTag(HttpUrl, request.GetDisplayUrl());
        activity.SetTag(HttpMethod, request.Method);
        activity.SetTag(HttpRequestContentLength, request.ContentLength);
        activity.SetTag(NetPeerName, request.Host.Host);
        
        DistributedContextPropagator.Current.Inject(activity, request, static (carrier, name, value) =>
        {
            if (carrier is HttpRequest r)
            {
                r.Headers.TryAdd(name, value);
            }
        });
       
    }
    
    public static void SetResponseTags(this Activity activity, HttpResponse response)
    {
        activity.SetTag(HttpStatusCode, response.StatusCode);
        activity.SetTag(HttpResponseContentLength, response.ContentLength);
    }
}