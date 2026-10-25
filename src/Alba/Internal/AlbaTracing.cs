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

    public const string PoolSlot = "alba.pool.slot";
    public const string PoolSlotGeneration = "alba.pool.slot_generation";
    public const string PoolWaitMilliseconds = "alba.pool.wait_ms";
    public const string PoolDiscarded = "alba.pool.discarded";

    public static Activity? StartPoolActivity(string operation)
    {
        if (!Source.HasListeners()) return null;

        return Source.StartActivity($"AlbaHostPool {operation}");
    }

    public static void RecordFailure(Activity? activity, Exception exception)
    {
        activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
    }

    public static Activity? StartRequestActivity(HttpRequest request)
    {
        var activity = Source.HasListeners()
            ? Source.StartActivity($"{request.Method} {request.Path}", ActivityKind.Client)
            : null;
        activity?.SetRequestTags(request);

        // Without an Alba activity, the test's own activity (such as the test framework's) still
        // parents the request and carries its baggage into the application
        injectTraceContext(activity ?? Activity.Current, request);

        return activity;
    }

    public static void SetRequestTags(this Activity activity, HttpRequest request)
    {
        activity.SetTag(HttpUrl, request.GetDisplayUrl());
        activity.SetTag(HttpMethod, request.Method);
        activity.SetTag(HttpRequestContentLength, request.ContentLength);
        activity.SetTag(NetPeerName, request.Host.Host);
    }

    private static void injectTraceContext(Activity? activity, HttpRequest request)
    {
        if (activity is null) return;

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