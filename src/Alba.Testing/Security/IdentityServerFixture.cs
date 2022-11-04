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
                .StartAsync();
        }

        public Task DisposeAsync()
        {
            _ = _host.StopAsync();
            return Task.CompletedTask;
        }

    }
}