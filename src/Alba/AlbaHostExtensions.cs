using System.Diagnostics.CodeAnalysis;
using Alba.Internal;
using Alba.Pooling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

 
namespace Alba;

public static class AlbaHostExtensions
{

    /// <summary>
    /// Configure an AlbaHost for a WebApplicationBuilder and WebApplication. The
    /// application starts when the returned builder is awaited
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configureRoutes">Configure the WebApplication for routing and/or middleware</param>
    /// <param name="extensions"></param>
    /// <returns></returns>
    public static AlbaHostBuilder StartAlbaAsync(this WebApplicationBuilder builder,
        Action<WebApplication> configureRoutes,
        params IAlbaExtension[] extensions)
    {
        return AlbaHost.For(builder, configureRoutes, extensions);
    }


    /// <summary>
    /// Configure an AlbaHost for the supplied IHostBuilder. The application starts
    /// when the returned builder is awaited
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="extensions"></param>
    /// <returns></returns>
    public static AlbaHostBuilder StartAlbaAsync(this IHostBuilder builder, params IAlbaExtension[] extensions)
    {
        return AlbaHost.For(builder, extensions);
    }

    /// <summary>
    ///     Shortcut to issue a POST with a Json serialized request body and a Json serialized
    ///     response body
    /// </summary>
    /// <param name="system"></param>
    /// <param name="request"></param>
    /// <param name="url"></param>
    /// <param name="jsonStyle"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static ResponseExpression PostJson<T>(this IAlbaHost system, T request, [StringSyntax(StringSyntaxAttribute.Uri)]string url, JsonStyle? jsonStyle = null) where T : class
    {
        return new(system, s => { s.Post.Json(request, jsonStyle).ToUrl(url); });
    }

    /// <summary>
    ///     Shortcut to issue a PUT with a Json serialized request body and a Json serialized
    ///     response body
    /// </summary>
    /// <param name="system"></param>
    /// <param name="request"></param>
    /// <param name="url"></param>
    /// <param name="jsonStyle"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static ResponseExpression PutJson<T>(this IAlbaHost system, T request, [StringSyntax(StringSyntaxAttribute.Uri)]string url, JsonStyle? jsonStyle = null) where T : class
    {
        return new(system, s => { s.Put.Json(request, jsonStyle).ToUrl(url); });
    }

    /// <summary>
    ///     Shortcut to just retrieve the contents of an HTTP GET as JSON and deserialize the resulting
    ///     response to the type "T"
    /// </summary>
    /// <param name="system"></param>
    /// <param name="url"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static async Task<T?> GetAsJson<T>(this IAlbaHost system, [StringSyntax(StringSyntaxAttribute.Uri)]string url)
    {
        var response = await system.Scenario(x => x.Get.Url(url).Accepts("application/json;text/json"));
        return await response.ReadAsJsonAsync<T>();
    }

    /// <summary>
    /// Issue a GET to the supplied url and read the response text as a string
    /// </summary>
    /// <param name="host"></param>
    /// <param name="url"></param>
    /// <returns></returns>
    public static async Task<string> GetAsText(this IAlbaHost host, [StringSyntax(StringSyntaxAttribute.Uri)]string url)
    {
        var response = await host.Scenario(x =>
        {
            x.Get.Url(url);
        });

        return await response.ReadAsTextAsync();
    }

    /// <summary>
    /// Find the host's extension of this type, including on a host rented from an AlbaHostPool.
    /// Pass a match to choose between several extensions of the type.
    /// </summary>
    /// <param name="host"></param>
    /// <param name="match"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">No extension, or more than one, matches</exception>
    public static T GetExtension<T>(this IAlbaHost host, Func<T, bool>? match = null) where T : IAlbaExtension
    {
        var found = ExtensionsOf(host).OfType<T>().Where(x => match?.Invoke(x) ?? true).ToArray();

        return found.Length switch
        {
            1 => found[0],
            0 => throw new InvalidOperationException(
                $"The host has no {typeof(T).Name} extension{(match is null ? "" : " that matches")}"),
            _ => throw new InvalidOperationException(
                $"The host has {found.Length} {typeof(T).Name} extensions{(match is null ? "" : " that match")}. Pass a match to choose one.")
        };
    }

    internal static IReadOnlyList<IAlbaExtension> ExtensionsOf(IAlbaHost host) => host switch
    {
        AlbaHostLease lease => lease.Inner.Extensions,
        AlbaHost albaHost => albaHost.Extensions,
        _ => []
    };

    /// <summary>
    /// Describes the application's endpoints, such as "GET /health", that anonymous requests
    /// can reach: endpoints marked [AllowAnonymous] or AllowAnonymous(), and endpoints with no
    /// authorization requirement when the application has no fallback authorization policy
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>
    public static IReadOnlyList<string> EndpointsAllowingAnonymous(this IAlbaHost host)
    {
        var fallbackPolicy = host.Services.GetService<IOptions<AuthorizationOptions>>()?.Value.FallbackPolicy;

        return EndpointDescriptions.DescribeAll(EndpointDescriptions.RoutableEndpoints(host.Services)
            .Where(e => allowsAnonymous(e, fallbackPolicy)));
    }

    // Mirrors the authorization middleware: [AllowAnonymous] always wins, otherwise an endpoint
    // without authorization metadata falls back to the fallback policy when there is one
    private static bool allowsAnonymous(Endpoint endpoint, AuthorizationPolicy? fallbackPolicy)
    {
        var metadata = endpoint.Metadata;
        if (metadata.GetMetadata<IAllowAnonymous>() != null) return true;

        var hasAuthorization = metadata.GetMetadata<IAuthorizeData>() != null
                               || metadata.GetMetadata<AuthorizationPolicy>() != null
                               || metadata.GetMetadata<IAuthorizationRequirementData>() != null;

        return !hasAuthorization && fallbackPolicy == null;
    }

    public class ResponseExpression
    {
        private readonly Action<Scenario> _configure;
        private readonly IAlbaHost _system;


        public ResponseExpression(IAlbaHost system, Action<Scenario> configure)
        {
            _system = system;
            _configure = configure;
        }

        public async Task<TResponse?> Receive<TResponse>()
        {
            var response = await _system.Scenario(_configure);
            return await response.ReadAsJsonAsync<TResponse>();
        }
    }
}