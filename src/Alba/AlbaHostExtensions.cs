using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

 
namespace Alba;

public static class AlbaHostExtensions
{

    /// <summary>
    /// Start an AlbaHost for a configured WebApplicationBuilder and WebApplication
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="configureRoutes">Configure the WebApplication for routing and/or middleware</param>
    /// <param name="extensions"></param>
    /// <returns></returns>
    public static Task<IAlbaHost> StartAlbaAsync(this WebApplicationBuilder builder,
        Action<WebApplication> configureRoutes,
        params IAlbaExtension[] extensions)
    {
        return AlbaHost.For(builder, configureRoutes, extensions);
    }

        
    /// <summary>
    /// Start an AlbaHost for the supplied IHostBuilder
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="extensions"></param>
    /// <returns></returns>
    public static Task<IAlbaHost> StartAlbaAsync(this IHostBuilder builder, params IAlbaExtension[] extensions)
    {
        return AlbaHost.For(builder, extensions);
    }

    public static IAlbaHost StartAlba(this IHostBuilder builder, params IAlbaExtension[] extensions)
    {
        return new AlbaHost(builder, extensions);
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
        return new(system, s =>
        {
            s.WriteJson(request, jsonStyle);
            s.Post.Json(request, jsonStyle).ToUrl(url);
        });
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