using System;
using System.Threading.Tasks;
using Alba.Security;
using IdentityServer;
using Shouldly;
using WebApi;
using Xunit;

namespace Alba.Testing.Security
{
    [Collection("OIDC")]
    public class OpenConnectUserPasswordTests : IDisposable
    {
        private readonly IdentityServerFixture _fixture;
        private readonly OpenConnectUserPassword oidc;
        private readonly IAlbaHost theHost;

        
        public OpenConnectUserPasswordTests(IdentityServerFixture fixture)
        {
            _fixture = fixture;
            
            oidc = new OpenConnectUserPassword
            {
                ClientId = Config.ClientId,
                ClientSecret = Config.ClientSecret,
                UserName = "alice",
                Password = "alice"
            };

            theHost = WebAppSecuredWithJwt.Program.CreateHostBuilder(new string[0])
                .StartAlba(oidc);
        }
        
        [Fact]
        public void assert_valid_happy_path()
        {
            var extensions = new OpenConnectUserPassword
            {
                ClientId = Guid.NewGuid().ToString(),
                ClientSecret = Guid.NewGuid().ToString(),
                UserName = "alice",
                Password = "alice"
            };

            extensions.AssertValid();
        }

        [Fact]
        public void not_valid_with_no_client_id()
        {
            var extensions = new OpenConnectUserPassword
            {
                //ClientId = Guid.NewGuid().ToString(),
                ClientSecret = Guid.NewGuid().ToString(),
                UserName = "alice",
                Password = "alice"
            };

            Should.Throw<Exception>(() => extensions.AssertValid());
        }
        
        [Fact]
        public void not_valid_with_no_client_secret()
        {
            var extensions = new OpenConnectUserPassword
            {
                ClientId = Guid.NewGuid().ToString(),
                //ClientSecret = Guid.NewGuid().ToString(),
                UserName = "alice",
                Password = "alice"
            };

            Should.Throw<Exception>(() => extensions.AssertValid());
        }
        
        [Fact]
        public void not_valid_with_no_username()
        {
            var extensions = new OpenConnectUserPassword
            {
                ClientId = Guid.NewGuid().ToString(),
                ClientSecret = Guid.NewGuid().ToString(),
                //UserName = "alice",
                Password = "alice"
            };

            Should.Throw<Exception>(() => extensions.AssertValid());
        }
        
        [Fact]
        public void not_valid_with_no_password()
        {
            var extensions = new OpenConnectUserPassword
            {
                ClientId = Guid.NewGuid().ToString(),
                ClientSecret = Guid.NewGuid().ToString(),
                UserName = "alice",
                //Password = "alice"
            };

            Should.Throw<Exception>(() => extensions.AssertValid());
        }

        [Fact]
        public async Task can_fetch_a_token()
        {
            var token = await oidc.FetchToken(null);

            token.ShouldNotBeNull();
            token.Error.ShouldBeNull();
        }
        
        [Fact]
        public async Task post_to_a_secured_endpoint_with_jwt_from_extension()
        {
            // Building the input body
            var input = new Numbers
            {
                Values = new[] {2, 3, 4}
            };

            var response = await theHost.Scenario(x =>
            {
                // Alba deals with Json serialization for us
                x.Post.Json(input).ToUrl("/math");
                
                // Enforce that the HTTP Status Code is 200 Ok
                x.StatusCodeShouldBeOk();
            });

            var output = response.ResponseBody.ReadAsJson<Result>();
            output.Sum.ShouldBe(9);
            output.Product.ShouldBe(24);
        }

        public void Dispose()
        {
            theHost?.Dispose();
        }
    }
}