using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
                context = await system.Invoke(c =>
                {
                    system.BeforeEach(c);

                    c.Request.Body.Position = 0;


                    scenario.SetupHttpContext(c);

                    if (c.Request.Path == null) throw new InvalidOperationException("This scenario has no defined url");
                });

                scenario.RunAssertions(context);
            }
            finally
            {
                system.AfterEach(context);
            }


            return new ScenarioResult(context, system);
        }


        [Obsolete("Move this to an extension method on IWebHostBuilder")]
        public static SystemUnderTest UseWindowsAuthentication(this SystemUnderTest system, ClaimsPrincipal user = null)
        {
            throw new NotImplementedException();

            //            system.Configure(c => {
//                c.ConfigureServices(s => {
//                    s.AddAuthentication(conf => {
//                        conf.DefaultAuthenticateScheme = "NTLM";
//                        conf.DefaultChallengeScheme = "Negotiate";
//                    })
//                        .AddScheme<AuthenticationSchemeOptions, StubNtlmAuthenticationHandlerV2>("NTLM", "NTLM", options =>  { })
//                        .AddScheme<AuthenticationSchemeOptions, StubNtlmAuthenticationHandlerV2>("Negotiate", "NTLM", options =>  { });
//                    s.AddSingleton(new StubNtlmAuthenticationHandlerV2(user));
//                });
//            });
            return system;
        }
    }
}