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
        /// Stubs out windows authentication to allow testing against systems that have windows authentication applied
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="user"></param>
        /// <returns></returns>
        public static IWebHostBuilder UseWindowsAuthentication(this IWebHostBuilder builder, ClaimsPrincipal user = null)
        {
            return builder.ConfigureServices(s =>
            {
                s.AddAuthentication(conf =>
                    {
                        conf.DefaultAuthenticateScheme = "NTLM";
                        conf.DefaultChallengeScheme = "Negotiate";
                    })
                    .AddScheme<AuthenticationSchemeOptions, StubNtlmAuthenticationHandlerV2>("NTLM", "NTLM",
                        options => { })
                    .AddScheme<AuthenticationSchemeOptions, StubNtlmAuthenticationHandlerV2>("Negotiate", "NTLM",
                        options => { });
                s.AddSingleton(new StubNtlmAuthenticationHandlerV2(user));
            });
        }
    }
}