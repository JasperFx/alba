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
    
    public class IdentityServerFixture : IDisposable
    {
        private readonly IHost _host;

        public IdentityServerFixture()
        {
            _host = IdentityServer.Program.CreateHostBuilder(Array.Empty<string>())
                .ConfigureServices(x => x.AddSingleton<IHostLifetime, NoopHostLifetime>())
                .Start();
        }

        public void Dispose()
        {
            _host?.Dispose();
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