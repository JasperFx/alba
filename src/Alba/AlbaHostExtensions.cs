using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

#nullable enable
namespace Alba
{
    public static class AlbaHostExtensions
    {
#if NET6_0_OR_GREATER
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
#endif
        
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
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ResponseExpression PostJson<T>(this IAlbaHost system, T request, string url) where T : class
        {
            return new(system, s =>
            {
                s.WriteRequestBody(request, MimeType.Json.Value);
                s.Post.Json(request).ToUrl(url);
            });
        }

        /// <summary>
        ///     Shortcut to issue a PUT with a Json serialized request body and a Json serialized
        ///     response body
        /// </summary>
        /// <param name="system"></param>
        /// <param name="request"></param>
        /// <param name="url"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ResponseExpression PutJson<T>(this IAlbaHost system, T request, string url) where T : class
        {
            return new(system, s => { s.Put.Json(request).ToUrl(url); });
        }

        /// <summary>
        ///     Shortcut to just retrieve the contents of an HTTP GET as JSON and deserialize the resulting
        ///     response to the type "T"
        /// </summary>
        /// <param name="system"></param>
        /// <param name="url"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<T?> GetAsJson<T>(this IAlbaHost system, string url)
        {
            var response = await system.Scenario(x => x.Get.Url(url).Accepts("application/json;text/json"));
            return response.ReadAsJson<T>();
        }

        /// <summary>
        /// Issue a GET to the supplied url and read the response text as a string
        /// </summary>
        /// <param name="host"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        public static async Task<string> GetAsText(this IAlbaHost host, string url)
        {
            var response = await host.Scenario(x =>
            {
                x.Get.Url(url);
            });

            return response.ReadAsText();
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
                return response.ReadAsJson<TResponse>();
            }
        }
    }
}