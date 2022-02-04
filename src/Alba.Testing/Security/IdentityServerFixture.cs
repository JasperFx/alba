using System;
using System.Threading.Tasks;
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

        public async Task DisposeAsync()
        {
            await _host.StopAsync();
            _host.Dispose();
        }
    }
}