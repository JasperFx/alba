using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace Alba.Testing.Security
{
    [CollectionDefinition("OIDC")]
    public class IdentityServerCollection : ICollectionFixture<IdentityServerFixture>
    {
        
    }
    
    public class IdentityServerFixture : IAsyncLifetime 
    {
        public TestServer IdentityServer { get; set; }
        public ValueTask InitializeAsync()
        {
            IdentityServer = new WebApplicationFactory<IdentityServer.New.Program>().Server;
            return ValueTask.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            IdentityServer.Dispose();
            return ValueTask.CompletedTask;
        }

    }
}