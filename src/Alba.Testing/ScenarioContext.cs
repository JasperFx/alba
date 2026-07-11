using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Alba.Testing
{
    public class ScenarioContext : IAsyncLifetime
    {
        protected CrudeRouter router = new CrudeRouter();
        protected IAlbaHost host = null!;

        public async ValueTask InitializeAsync()
        {
            host = await AlbaHost.For(Host.CreateDefaultBuilder()
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

        public async ValueTask DisposeAsync()
        {
            if (host != null) await host.DisposeAsync();
        }
    }
}