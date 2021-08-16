using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Alba.Security;
using Microsoft.IdentityModel.JsonWebTokens;
using Shouldly;
using WebApi;
using WebAppSecuredWithJwt;
using Xunit;

namespace Alba.Testing.Security
{
    public class web_api_authentication_with_stub : IDisposable
    {
        private readonly IAlbaHost theHost;

        public web_api_authentication_with_stub()
        {
            #region sample_bootstrapping_with_stub_extension

            // This is calling your real web service's configuration
            var hostBuilder = Program.CreateHostBuilder(new string[0]);

            // This is a new Alba v5 extension that can "stub" out
            // JWT token authentication
            var securityStub = new AuthenticationStub()
                .With("foo", "bar")
                .With(JwtRegisteredClaimNames.Email, "guy@company.com")
                .WithName("jeremy");

            // AlbaHost was "SystemUnderTest" in previous versions of
            // Alba
            theHost = new AlbaHost(hostBuilder, securityStub);

            #endregion
        }


        public void Dispose()
        {
            theHost?.Dispose();
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
                x.Post.Json(input).ToUrl("/math");
                x.StatusCodeShouldBeOk();
            });

            var principal = response.Context.User;
            principal.ShouldNotBeNull();
            
            principal.Claims.Single(x => x.Type == "color")
                .Value.ShouldBe("green");
        }

        #endregion
    }
}