using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

#if NETSTANDARD2_0

#endif

namespace Alba
{
    public static class SystemUnderTestExtensions
    {
        // SAMPLE: ScenarioSignature
        /// <summary>
        ///     Define and execute an integration test by running an Http request through
        ///     your ASP.Net Core system
        /// </summary>
        /// <param name="system"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static async Task<IScenarioResult> Scenario(
                this ISystemUnderTest system,
                Action<Scenario> configure)
            // ENDSAMPLE
        {
            var scenario = new Scenario(system);


            configure(scenario);

            scenario.Rewind();

            HttpContext context = null;
            try
            {
                context = await system.Invoke(async c =>
                {
                    await system.BeforeEach(c);

                    c.Request.Body.Position = 0;


                    scenario.SetupHttpContext(c);

                    if (c.Request.Path == null) throw new InvalidOperationException("This scenario has no defined url");
                });

                scenario.RunAssertions(context);
            }
            finally
            {
                await system.AfterEach(context);
            }

            if (context.Response.Body.CanSeek)
            {
                context.Response.Body.Position = 0;
            }


            return new ScenarioResult(context, system);
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
        public static ResponseExpression PostJson<T>(this ISystemUnderTest system, T request, string url) where T : class
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
        public static ResponseExpression PutJson<T>(this ISystemUnderTest system, T request, string url) where T : class
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
        public static async Task<T> GetAsJson<T>(this ISystemUnderTest system, string url)
        {
            var response = await system.Scenario(x => x.Get.Url(url).Accepts("application/json;text/json"));
            return response.ResponseBody.ReadAsJson<T>();
        }

        public class ResponseExpression
        {
            private readonly ISystemUnderTest _system;
            private readonly Action<Scenario> _configure;


            public ResponseExpression(ISystemUnderTest system, Action<Scenario> configure)
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