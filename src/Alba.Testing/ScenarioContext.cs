using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Alba.Testing
{
    public class ScenarioContext : IDisposable
    {
        protected CrudeRouter router = new CrudeRouter();
        protected readonly IAlbaHost host;

        public ScenarioContext()
        {
            host = new AlbaHost(Host.CreateDefaultBuilder()
                .ConfigureServices((s) => s.AddMvcCore())
                .ConfigureWebHostDefaults(c =>
                c.Configure(app =>
                {
                    app.Run(router.Invoke);
                })));            
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