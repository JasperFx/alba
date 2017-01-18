using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Alba
{
    public static class SystemUnderTestExtensions
    {
        // How can we do this in such a way that you can use StructureMap child containers
        // for test isolation?
        public static async Task<Scenario> Scenario(this ISystemUnderTest system, Action<Scenario> configure)
        {
            using (var scope = system.Services.GetService<IServiceScopeFactory>().CreateScope())
            {
                var scenario = new Scenario(system, scope);
                configure(scenario);

                try
                {
                    await system.BeforeEach(scenario.Context).ConfigureAwait(false);

                    await scenario.RunBeforeActions().ConfigureAwait(false);

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