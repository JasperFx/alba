using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
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
        public TestServer IdentityServer { get; set; }
        public Task InitializeAsync()
        {
            IdentityServer = new WebApplicationFactory<IdentityServer.New.Program>().Server;
            return Task.CompletedTask;
        }

        public Task DisposeAsync()
        {
            IdentityServer.Dispose();
            return Task.CompletedTask;
        }

    }
}