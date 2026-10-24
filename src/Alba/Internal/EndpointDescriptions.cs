using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace Alba.Internal;

internal static class EndpointDescriptions
{
    /// <summary>
    /// Every endpoint that a request can be routed to
    /// </summary>
    public static IEnumerable<RouteEndpoint> RoutableEndpoints(IServiceProvider services)
    {
        return (services.GetService<EndpointDataSource>()?.Endpoints ?? [])
            .OfType<RouteEndpoint>()
            .Where(e => e.Metadata.GetMetadata<ISuppressMatchingMetadata>()?.SuppressMatching != true);
    }

    /// <summary>
    /// Describes the endpoints in a stable order
    /// </summary>
    public static IReadOnlyList<string> DescribeAll(IEnumerable<RouteEndpoint> endpoints)
    {
        return endpoints.Select(Describe).OrderBy(x => x, StringComparer.Ordinal).ToList();
    }

    /// <summary>
    /// Identifies an endpoint across data sources, because minimal API data sources
    /// create new endpoint instances for every caller
    /// </summary>
    public static string Key(RouteEndpoint endpoint)
    {
        return $"{Describe(endpoint)}|{endpoint.DisplayName}|{endpoint.Order}";
    }

    /// <summary>
    /// Describes the endpoint as its HTTP methods and route pattern, such as "GET /accounts/{id}"
    /// </summary>
    public static string Describe(RouteEndpoint endpoint)
    {
        var pattern = endpoint.RoutePattern.RawText ?? endpoint.DisplayName ?? "(unknown pattern)";
        if (!pattern.StartsWith('/') && endpoint.RoutePattern.RawText != null) pattern = "/" + pattern;

        var methods = endpoint.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods;

        return methods is { Count: > 0 } ? $"{string.Join(", ", methods)} {pattern}" : pattern;
    }
}
