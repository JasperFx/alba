using Alba.Security;
using IdentityServer.New;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using WebApi;

namespace Alba.Testing.Security
{
    [Collection("OIDC")]
    public class OpenConnectClientCredentialsTests : IAsyncLifetime
    {
        private IdentityServerFixture _fixture;
        private OpenConnectClientCredentials oidc = null!;
        private IAlbaHost theHost = null!;

        public OpenConnectClientCredentialsTests(IdentityServerFixture fixture)
        {
            _fixture = fixture;
        }

        public async ValueTask InitializeAsync()
        {
            #region sample_OpenConnectClientCredentials

            oidc = new OpenConnectClientCredentials
            {
                // These three properties are mandatory, and
                // would refer to matching configuration in your
                // OIDC server
                ClientId = Config.ClientId,
                ClientSecret = Config.ClientSecret,
                Scope = Config.ApiScope
            };

            theHost = await AlbaHost.For<WebAppSecuredWithJwt.Program>(x =>
            {
                x.ConfigureServices((ctx, collection) =>
                    collection.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme,
                        options =>
                        {
                            options.Authority = _fixture.IdentityServer.BaseAddress.ToString();
                            options.BackchannelHttpHandler = _fixture.IdentityServer.CreateHandler();
                            options.RequireHttpsMetadata = false;
                        }));
            }, oidc);
            #endregion
        }

        public async ValueTask DisposeAsync()
        {
            await theHost.DisposeAsync();
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

        [Fact]
        public async Task can_fetch_a_token()
        {
            var token = await oidc.FetchToken(null);

            token.ShouldNotBeNull();
        }

        [Fact]
        public async Task post_to_a_secured_endpoint_with_jwt_from_extension()
        {
            // Building the input body
            var input = new Numbers
            {
                Values = new[] { 2, 3, 4 }
            };

            var response = await theHost.Scenario(x =>
            {
                // Alba deals with Json serialization for us
                x.Post.Json(input).ToUrl("/math");

                // Enforce that the HTTP Status Code is 200 Ok
                x.StatusCodeShouldBeOk();
            });

            var output = await response.ReadAsJsonAsync<Result>();
            output.Sum.ShouldBe(9);
            output.Product.ShouldBe(24);
        }
    }
}