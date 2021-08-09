using System;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Alba.Jwt;
using Microsoft.IdentityModel.JsonWebTokens;
using Shouldly;
using WebApi;
using WebAppSecuredWithJwt;
using Xunit;

namespace Alba.Testing.Jwt
{
    public class web_api_authentication : IDisposable
    {
        private readonly IAlbaHost theHost;

        public web_api_authentication()
        {
            var hostBuilder = Program.CreateHostBuilder(new string[0]);


            var jwtSecurityStub = new JwtSecurityStub()
                .With("foo", "bar")
                .With(JwtRegisteredClaimNames.Email, "guy@company.com");

            theHost = new AlbaHost(hostBuilder, jwtSecurityStub);
        }


        public void Dispose()
        {
            theHost?.Dispose();
        }

        [Fact]
        public async Task should_be_401_with_no_jwt()
        {
            var input = new Numbers
            {
                Values = new[] {2, 3, 4}
            };

            await theHost.Scenario(x =>
            {
                // override the StubJwt behavior to verify the authentication
                // really exists
                x.RemoveRequestHeader("Authorization");
                x.Post.Json(input).ToUrl("/math");
                x.StatusCodeShouldBe(HttpStatusCode.Unauthorized);
            });
        }

        [Fact]
        public async Task post_to_a_secured_endpoint_with_jwt_from_extension()
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

            var output = response.ResponseBody.ReadAsJson<Result>();
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
            principal.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Aud).Value.ShouldBe("jwtsample");
        }

        [Fact]
        public async Task can_modify_claims_per_scenario()
        {
            var input = new Numbers
            {
                Values = new[] {2, 3, 4}
            };

            var response = await theHost.Scenario(x =>
            {
                x.WithClaim(new Claim("color", "green"));
                x.Post.Json(input).ToUrl("/math");
                x.StatusCodeShouldBeOk();
            });

            var principal = response.Context.User;
            principal.ShouldNotBeNull();
            
            principal.Claims.Single(x => x.Type == "color").Value.ShouldBe("green");
        }
    }
}