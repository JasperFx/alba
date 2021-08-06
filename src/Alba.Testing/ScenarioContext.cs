using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Alba.Testing
{
    public class ScenarioContext : IDisposable
    {
        protected CrudeRouter router = new CrudeRouter();
        protected readonly IScenarioRunner host;

        public ScenarioContext()
        {
            host = new SystemUnderTest(Host.CreateDefaultBuilder().ConfigureWebHostDefaults(c =>
                c.Configure(app => { app.Run(router.Invoke); })));
            host.Urls = router;
        }


        protected Task<ScenarioAssertionException> fails(Action<Scenario> configuration)
        {
            return Exception<ScenarioAssertionException>.ShouldBeThrownBy(() => host.Scenario(configuration));
        }

        public void Dispose()
        {
            host?.Dispose();
        }
    }
}