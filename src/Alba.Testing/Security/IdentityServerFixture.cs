using System;
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
            _host = IdentityServer.Program.CreateHostBuilder(new string[0])
                .Start();
        }

        public void Dispose()
        {
            _host?.Dispose();
        }
    }
}