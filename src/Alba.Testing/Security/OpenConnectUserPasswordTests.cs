using Alba.Security;
using IdentityServer.New;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using WebApi;

namespace Alba.Testing.Security
{
    [Collection("OIDC")]
    public class OpenConnectUserPasswordTests : IAsyncLifetime
    {
        private readonly IdentityServerFixture _fixture;
        private OpenConnectUserPassword oidc = null!;
        private IAlbaHost theHost = null!;


        public OpenConnectUserPasswordTests(IdentityServerFixture fixture)
        {
            _fixture = fixture;
        }

        public async ValueTask InitializeAsync()
        {
            #region sample_applying_OpenConnectUserPassword

            oidc = new OpenConnectUserPassword
            {
                // All of these properties are mandatory
                ClientId = Config.ClientId,
                ClientSecret = Config.ClientSecret,
                UserName = "alice",
                Password = "alice"
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

            var output = response.ReadAsJson<Result>();
            output.Sum.ShouldBe(9);
            output.Product.ShouldBe(24);

            var user = response.Context.User;
            user.FindFirst("name").Value.ShouldBe("Alice Smith");
        }


        #region sample_override_user_password

        [Fact]
        public async Task post_to_a_secured_endpoint_with_jwt_with_overridden_user_and_password()
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
                
                // Override the user credentials for just this scenario
                x.UserAndPasswordIs("bob", "bob");
                
                // Enforce that the HTTP Status Code is 200 Ok
                x.StatusCodeShouldBeOk();
            });

            var output = response.ReadAsJson<Result>();
            output.Sum.ShouldBe(9);
            output.Product.ShouldBe(24);

            var user = response.Context.User;
            user.FindFirst("name").Value.ShouldBe("Bob Smith");
        }

        #endregion
    }
}