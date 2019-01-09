using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Alba.Testing
{
    public class ScenarioContext
    {
        protected CrudeRouter router = new CrudeRouter();
        protected readonly ISystemUnderTest host;

        public ScenarioContext()
        {
            host = new SystemUnderTest(new WebHostBuilder().Configure(app => app.Run(router.Invoke)));
            host.Urls = router;
        }
        
        

        protected Task<ScenarioAssertionException> fails(Action<Scenario> configuration)
        {
            return Exception<ScenarioAssertionException>.ShouldBeThrownBy(() => host.Scenario(configuration));
        }
    }
}