using System.Security.Claims;
using System.Security.Cryptography;
using Alba.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Shouldly;

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
                .Subject.Claims.ShouldContain(x => x.Type == JwtRegisteredClaimNames.Jti);
        }

        [Fact]
        public void should_have_audience()
        {
            var token = theStub.BuildToken();
            token.Audience.ShouldBe("chiefsfans");
        }

        [Fact]
        public void should_have_issuer_if_it_exists()
        {
            theStub.BuildToken().Subject.Claims.First(x => x.Type == JwtRegisteredClaimNames.Iss)
                .Value.ShouldBe("myapp");
        }

        [Fact]
        public void has_all_the_baseline_claims()
        {
            var token = theStub.BuildToken();

            token
                .Subject.Claims.Single(x => x.Type == "foo")
                .Value.ShouldBe("bar");

            token
                .Subject.Claims.Single(x => x.Type == "team")
                .Value.ShouldBe("chiefs");
        }

        [Fact]
        public void additive_claims_on_the_token()
        {
            var token = theStub.BuildToken(new []{new Claim("division", "afcwest")});

            token
                .Subject.Claims.Single(x => x.Type == "division")
                .Value.ShouldBe("afcwest");
        }
        
        
        [Fact]
        public void should_handle_non_hmac_signing_key()
        {
            using var ecdsa = ECDsa.Create();
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                        .AddJwtBearer(opt =>
                        {
                            ecdsa.ImportFromPem(@"-----BEGIN EC PRIVATE KEY-----
MHcCAQEEIKTr/pgYcyfYBKfYvAlFhMRvEtCFx53kuLvd7T/IPi0AoAoGCCqGSM49
AwEHoUQDQgAEyUjhuVjVf/xyrlizuGdXCu0CKERSLP+DkO+DjpzcC3oa4+HJaOoR
z/iMv39jDM5WBfFLh32DmBzDKPaAq7yMXA==
-----END EC PRIVATE KEY-----");
                            var signingKey = new ECDsaSecurityKey(ecdsa);
                            opt.TokenValidationParameters = new TokenValidationParameters
                            {
                                IssuerSigningKey = signingKey,
                                ValidAlgorithms = new[] {"ES256"}
                            };
                        });
                })
                .StartAlba(theStub);

            Action act = () => theStub.BuildJwtString(Array.Empty<Claim>());
            
            act.ShouldNotThrow();
        }

        public void Dispose()
        {
            _host?.Dispose();
        }
    }
}