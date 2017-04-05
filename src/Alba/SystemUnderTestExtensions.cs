using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

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
    }
}