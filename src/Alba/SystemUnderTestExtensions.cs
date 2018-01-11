using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

#if NETSTANDARD2_0
using Microsoft.AspNetCore.Authentication;
#endif

namespace Alba
{
    public static class SystemUnderTestExtensions
    {
        // SAMPLE: ScenarioSignature
        /// <summary>
        /// Define and execute an integration test by running an Http request through
        /// your ASP.Net Core system
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
            using (var scope = system.Services.GetService<IServiceScopeFactory>().CreateScope())
            {
                var scenario = new Scenario(system, scope);

                var contextAccessor = scope.ServiceProvider.GetService<IHttpContextAccessor>();
                contextAccessor.HttpContext = scenario.Context;

                configure(scenario);

                scenario.Rewind();

                try
                {
                    await system.BeforeEach(scenario.Context).ConfigureAwait(false);

                    if (scenario.Context.Request.Path == null)
                    {
                        throw new InvalidOperationException("This scenario has no defined url");
                    }

                    await system.Invoker(scenario.Context).ConfigureAwait(false);

                    scenario.Context.Response.Body.Position = 0;

                    scenario.RunAssertions();
                }
                finally
                {
                    await system.AfterEach(scenario.Context).ConfigureAwait(false);
                }


                return scenario;
            }
        }


        public static SystemUnderTest UseWindowsAuthentication(this SystemUnderTest system, ClaimsPrincipal user = null) {
#if NETSTANDARD2_0
            system.Configure(c => {
                c.ConfigureServices(s => {
                    s.AddAuthentication(conf => {
                        conf.DefaultAuthenticateScheme = "NTLM";
                        conf.DefaultChallengeScheme = "Negotiate";
                    })
                        .AddScheme<AuthenticationSchemeOptions, StubNtlmAuthenticationHandlerV2>("NTLM", "NTLM", options =>  { })
                        .AddScheme<AuthenticationSchemeOptions, StubNtlmAuthenticationHandlerV2>("Negotiate", "NTLM", options =>  { });
                    s.AddSingleton(new StubNtlmAuthenticationHandlerV2(user));
                });
            });
#endif
            return system;
        }
    }
}