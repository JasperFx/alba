using System.Security.Claims;
using Alba.Security;
using Microsoft.IdentityModel.JsonWebTokens;
using Shouldly;
using WebApi;

namespace Alba.Testing.Security
{
    public class web_api_authentication_with_stub : IAsyncLifetime
    {
        private IAlbaHost theHost = null!;

        public async ValueTask InitializeAsync()
        {
            #region sample_bootstrapping_with_stub_extension
            // This is a Alba extension that can "stub" out authentication
            var securityStub = new AuthenticationStub()
                .With("foo", "bar")
                .With(JwtRegisteredClaimNames.Email, "guy@company.com")
                .WithName("jeremy");

            // We're calling your real web service's configuration
            theHost = await AlbaHost.For<WebAppSecuredWithJwt.Program>(securityStub);


            #endregion
        }

        public async ValueTask DisposeAsync()
        {
            await theHost.DisposeAsync();
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
        }

        [Fact]
        public async Task have_the_baseline_claims_on_the_principal()
        {
            var input = new Numbers
            {
                Values = new[] {2, 3, 4}
            };

            var response = await theHost.Scenario(x =>
            {
                x.Post.Json(input).ToUrl("/math");
                x.StatusCodeShouldBeOk();
            });

            var principal = response.Context.User;
            principal.ShouldNotBeNull();
            principal.Claims.Single(x => x.Type == "foo").Value.ShouldBe("bar");
            principal.Claims.Single(x => x.Type == "email").Value.ShouldBe("guy@company.com");
            principal.Identity.Name.ShouldBe("jeremy");
        }

        #region sample_specify_specific_claims

        [Fact]
        public async Task can_modify_claims_per_scenario()
        {
            var input = new Numbers
            {
                Values = new[] {2, 3, 4}
            };

            var response = await theHost.Scenario(x =>
            {
                // This is a custom claim that would only be used for the 
                // JWT token in this individual test
                x.WithClaim(new Claim("color", "green"));
                x.RemoveClaim("foo");
                x.Post.Json(input).ToUrl("/math");
                x.StatusCodeShouldBeOk();
            });

            var principal = response.Context.User;
            principal.ShouldNotBeNull();
            
            principal.Claims.Single(x => x.Type == "color")
                .Value.ShouldBe("green");

            principal.Claims.Any(x => x.Type.Equals("foo")).ShouldBeFalse();
        }

        #endregion
      
    }
}