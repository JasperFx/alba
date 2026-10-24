using System.Collections.Concurrent;
using System.Diagnostics;
using Alba.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Alba;

/// <summary>
/// Alba extension that records which of the application's endpoints requests are routed to.
/// Pass one instance to every AlbaHost of the application, such as every host in a pool, and
/// check NotExercised() once all the tests have run. Results survive the hosts being disposed.
/// </summary>
public sealed class EndpointCoverage : IAlbaExtension
{
    // Written to the host's DiagnosticListener by the routing middleware with the HttpContext
    private const string EndpointMatched = "Microsoft.AspNetCore.Routing.EndpointMatched";

    // Endpoint instances differ between hosts, and between the matcher and the data source,
    // so endpoints are identified by key
    private readonly ConcurrentDictionary<string, string> _endpoints = new();
    private readonly ConcurrentDictionary<string, byte> _exercised = new();

    /// <summary>
    /// Describes the endpoints, such as "GET /accounts/{id}", that no request through any of
    /// the hosts has been routed to so far. An endpoint counts as soon as routing selects it,
    /// whatever the response.
    /// </summary>
    /// <returns></returns>
    public IReadOnlyList<string> NotExercised()
    {
        return _endpoints
            .Where(x => !_exercised.ContainsKey(x.Key))
            .Select(x => x.Value)
            .OrderBy(x => x, StringComparer.Ordinal)
            .ToList();
    }

    Task IAlbaExtension.Start(IAlbaHost host)
    {
        foreach (var endpoint in EndpointDescriptions.RoutableEndpoints(host.Services))
        {
            _endpoints.TryAdd(EndpointDescriptions.Key(endpoint), EndpointDescriptions.Describe(endpoint));
        }

        var subscription = host.Services.GetService<DiagnosticListener>()
            ?.Subscribe(new HostObserver(this), name => name == EndpointMatched);

        // Other hosts can still be sharing this instance, so each host only ends its own subscription
        if (subscription != null)
        {
            host.Services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopped
                .Register(subscription.Dispose);
        }

        return Task.CompletedTask;
    }

    void IDisposable.Dispose()
    {
    }

    ValueTask IAsyncDisposable.DisposeAsync() => ValueTask.CompletedTask;

    private sealed class HostObserver(EndpointCoverage coverage) : IObserver<KeyValuePair<string, object?>>
    {
        // Routing reuses its endpoint instances, so each key is built once per host
        private readonly ConcurrentDictionary<Endpoint, byte> _seen = new();

        public void OnNext(KeyValuePair<string, object?> value)
        {
            if (value.Value is HttpContext context && context.GetEndpoint() is RouteEndpoint endpoint
                                                   && _seen.TryAdd(endpoint, 0))
            {
                coverage._exercised.TryAdd(EndpointDescriptions.Key(endpoint), 0);
            }
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }
    }
}
