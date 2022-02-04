using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Alba.Testing.Security
{
    [CollectionDefinition("OIDC")]
    public class IdentityServerCollection : ICollectionFixture<IdentityServerFixture>
    {
        
    }
    
    public class IdentityServerFixture : IAsyncLifetime
    {
        private IHost _host;

        public async Task InitializeAsync()
        {
            _host = await IdentityServer.Program.CreateHostBuilder(Array.Empty<string>())
                .ConfigureServices(x=> x.AddSingleton<IHostLifetime, NoopHostLifetime>())
                .StartAsync();
        }

        public Task DisposeAsync()
        {
            _host.Dispose();
            return Task.CompletedTask;
        }
    }

    internal class NoopHostLifetime : IHostLifetime
    {
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task WaitForStartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}