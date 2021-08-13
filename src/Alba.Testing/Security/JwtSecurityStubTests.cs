using System;
using System.Linq;
using System.Security.Claims;
using Alba.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.JsonWebTokens;
using Shouldly;
using Xunit;

namespace Alba.Testing.Security
{
    public class JwtSecurityStubTests : IDisposable
    {
        private readonly JwtSecurityStub theStub;
        private readonly IAlbaHost _host;

        public JwtSecurityStubTests()
        {
            theStub = new JwtSecurityStub()
                .With("foo", "bar")
                .With("team", "chiefs");



            _host = Host.CreateDefaultBuilder().StartAlba(theStub);
            
            theStub.Options = new JwtBearerOptions
            {
                ClaimsIssuer = "myapp",
                Audience = "chiefsfans"
            };
        }

        [Fact]
        public void token_has_identifier()
        {
            theStub.BuildToken()
                .Claims.ShouldContain(x => x.Type == JwtRegisteredClaimNames.Jti);
        }

        [Fact]
        public void should_have_audience()
        {
            var token = theStub.BuildToken();
            token
                .Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Aud)
                .Value.ShouldBe("chiefsfans");
        }

        [Fact]
        public void should_have_issuer_if_it_exists()
        {
            theStub.BuildToken()
                .Claims.FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Iss)
                .Value.ShouldBe("myapp");
        }

        [Fact]
        public void has_all_the_baseline_claims()
        {
            var token = theStub.BuildToken();
            
            token.Claims.Single(x => x.Type == "foo")
                .Value.ShouldBe("bar");
            
            token.Claims.Single(x => x.Type == "team")
                .Value.ShouldBe("chiefs");
        }

        [Fact]
        public void additive_claims_on_the_token()
        {
            var token = theStub.BuildToken(new Claim("division", "afcwest"));
            
            token.Claims.Single(x => x.Type == "division")
                .Value.ShouldBe("afcwest");
        }

        public void Dispose()
        {
            _host?.Dispose();
        }
    }
}