using System;
using Alba.Security;
using Shouldly;
using Xunit;

namespace Alba.Testing.Security
{
    [Collection("OIDC")]
    public class OpenConnectClientCredentialsTests 
    {
        private readonly IdentityServerFixture _fixture;

        public OpenConnectClientCredentialsTests(IdentityServerFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public void assert_valid_happy_path()
        {
            var extensions = new OpenConnectClientCredentials
            {
                ClientId = Guid.NewGuid().ToString(),
                ClientSecret = Guid.NewGuid().ToString(),
                Scope = Guid.NewGuid().ToString()
            };

            extensions.AssertValid();
        }

        [Fact]
        public void not_valid_with_no_client_id()
        {
            var extensions = new OpenConnectClientCredentials
            {
                //ClientId = Guid.NewGuid().ToString(),
                ClientSecret = Guid.NewGuid().ToString(),
                Scope = Guid.NewGuid().ToString()
            };

            Should.Throw<Exception>(() => extensions.AssertValid());
        }
        
        [Fact]
        public void not_valid_with_no_client_secret()
        {
            var extensions = new OpenConnectClientCredentials
            {
                ClientId = Guid.NewGuid().ToString(),
                //ClientSecret = Guid.NewGuid().ToString(),
                Scope = Guid.NewGuid().ToString()
            };

            Should.Throw<Exception>(() => extensions.AssertValid());
        }
        
        [Fact]
        public void not_valid_with_no_scope()
        {
            var extensions = new OpenConnectClientCredentials
            {
                ClientId = Guid.NewGuid().ToString(),
                ClientSecret = Guid.NewGuid().ToString(),
                //Scope = Guid.NewGuid().ToString()
            };

            Should.Throw<Exception>(() => extensions.AssertValid());
        }
    }
}