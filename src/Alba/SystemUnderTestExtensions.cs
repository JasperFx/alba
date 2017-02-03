using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Alba
{
    public static class SystemUnderTestExtensions
    {
        // How can we do this in such a way that you can use StructureMap child containers
        // for test isolation?
        public static async Task<IScenarioResult> Scenario(this ISystemUnderTest system, Action<Scenario> configure)
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

                    await scenario.RunBeforeActions().ConfigureAwait(false);

                    if (scenario.Context.Request.Path == null)
                    {
                        throw new InvalidOperationException("This scenario has no defined url");
                    }

                    await system.Invoker(scenario.Context).ConfigureAwait(false);

                    scenario.RunAssertions();

                    await scenario.RunAfterActions().ConfigureAwait(false);
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