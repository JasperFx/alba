using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace Alba
{
    public static class SystemUnderTestExtensions
    {
        public static Task<IAlbaHost> StartAlbaHostAsync(this IHostBuilder builder)
        {
            return AlbaHost.For(builder);
        }

        /// <summary>
        /// Shortcut to issue a POST with a Json serialized request body and a Json serialized
        /// response body
        /// </summary>
        /// <param name="system"></param>
        /// <param name="request"></param>
        /// <param name="url"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ResponseExpression PostJson<T>(this IAlbaHost system, T request, string url) where T : class
        {
            return new ResponseExpression(system, s =>
            {
                s.Post.Json(request).ToUrl(url);
            });
        }
        
        /// <summary>
        /// Shortcut to issue a PUT with a Json serialized request body and a Json serialized
        /// response body
        /// </summary>
        /// <param name="system"></param>
        /// <param name="request"></param>
        /// <param name="url"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ResponseExpression PutJson<T>(this IAlbaHost system, T request, string url) where T : class
        {
            return new ResponseExpression(system, s =>
            {
                s.Put.Json(request).ToUrl(url);
            });
        }

        /// <summary>
        /// Shortcut to just retrieve the contents of an HTTP GET as JSON and deserialize the resulting
        /// response to the type "T"
        /// </summary>
        /// <param name="system"></param>
        /// <param name="url"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static async Task<T> GetAsJson<T>(this IAlbaHost system, string url)
        {
            var response = await system.Scenario(x => x.Get.Url(url).Accepts("application/json;text/json"));
            return response.ResponseBody.ReadAsJson<T>();
        }

        public class ResponseExpression
        {
            private readonly IAlbaHost _system;
            private readonly Action<Scenario> _configure;


            public ResponseExpression(IAlbaHost system, Action<Scenario> configure)
            {
                _system = system;
                _configure = configure;
            }

            public async Task<TResponse> Receive<TResponse>()
            {
                var response = await _system.Scenario(_configure);
                return response.ResponseBody.ReadAsJson<TResponse>();
            }
        }

    }
}