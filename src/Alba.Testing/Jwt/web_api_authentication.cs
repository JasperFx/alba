using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Shouldly;
using WebApi;
using Xunit;

namespace Alba.Testing.Jwt
{
    // public class StubIdentityServer : IDisposable
    // {
    //     public StubIdentityServer(int port)
    //     {
    //         AuthorityUrl = $"http://localhost:{port}";
    //
    //     }
    //
    //     public string AuthorityUrl { get; }
    //
    //     public Task<IHostBuilder> Attach(IHostBuilder application)
    //     {
    //         var setUrl = new SetAuthorityUrl(AuthorityUrl);
    //         application = setUrl.Register(application);
    //         
    //         
    //     }
    // }
    
    internal class SetAuthorityUrl : IPostConfigureOptions<JwtBearerOptions>
    {
        private readonly string _url;

        public SetAuthorityUrl(string url)
        {
            _url = url;
        }

        public IHostBuilder Register(IHostBuilder builder)
        {
            return builder.ConfigureServices((c, s) =>
            {
                s.AddSingleton<IPostConfigureOptions<JwtBearerOptions>>(this);
            });
        }

        public void PostConfigure(string name, JwtBearerOptions options)
        {
            // This will deactivate the callout to the OIDC server
            options.ConfigurationManager =
                new StaticConfigurationManager<OpenIdConnectConfiguration>(new OpenIdConnectConfiguration
                {
                    
                });
            
            var original = options.TokenValidationParameters;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                IssuerSigningKey = original.IssuerSigningKey,
                ValidIssuer = original.ValidIssuer,
                ValidAudience = original.ValidAudience,
                ValidateIssuer = false
            };
            options.Authority = null;
            options.MetadataAddress = null;
        }
    }
    
    public class web_api_authentication : IDisposable
    {
        private readonly IAlbaHost theSystem;

        public web_api_authentication()
        {
            var hostBuilder = WebAppSecuredWithJwt.Program.CreateHostBuilder(new string[0])
                .ConfigureServices((e, services) =>
                {
                    services.AddSingleton<IPostConfigureOptions<JwtBearerOptions>>(new SetAuthorityUrl("http://localhost:5003"));
                });
                ;
            theSystem = new AlbaHost(hostBuilder);
        }



        public void Dispose()
        {
            theSystem?.Dispose();
        }

        [Fact]
        public async Task should_be_401_with_no_jwt()
        {
            var input = new Numbers
            {
                Values = new[] {2, 3, 4}
            };

            await theSystem.Scenario(x =>
            {
                x.Post.Json(input).ToUrl("/math");
                x.StatusCodeShouldBe(HttpStatusCode.Unauthorized);
            });
        }

        [Fact]
        public async Task post_to_a_secured_endpoint()
        {
            var options = theSystem.Services.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
                .Get("Bearer");
            Debug.WriteLine(options);
            
            
            
            var harness = new JwtTestHarness(options);
            var jwt = harness.BuildJwtString();
            
            var input = new Numbers
            {
                Values = new[] {2, 3, 4}
            };

            var response = await theSystem.Scenario(x =>
            {
                x.Post.Json(input).ToUrl("/math");
                x.StatusCodeShouldBeOk();
                x.SetRequestHeader("Authorization", $"Bearer {jwt}");
            });

            var output = response.ResponseBody.ReadAsJson<Result>();
            output.Sum.ShouldBe(9);
            output.Product.ShouldBe(24);
        }
    }

    public class JwtTestHarness
    {
        private readonly JwtBearerOptions _options;

        public JwtTestHarness(JwtBearerOptions options)
        {
            _options = options;
        }

        public List<Claim> BaselineClaims { get; } = new List<Claim>();

        public JwtSecurityToken BuildToken(params Claim[] claims)
        {
            // TODO -- get the algorithm from the options validation parameters,
            // but use HmacSha256 as the default
            var credentials = new SigningCredentials(_options.TokenValidationParameters.IssuerSigningKey,
                SecurityAlgorithms.HmacSha256);

            return new JwtSecurityToken(_options.ClaimsIssuer, _options.Audience, allClaims(claims),
                expires: DateTime.UtcNow.AddDays(1), signingCredentials: credentials);
        }

        public IEnumerable<Claim> allClaims(Claim[] claims)
        {
            yield return new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString());
            if (_options.ClaimsIssuer != null)
            {
                yield return new Claim(JwtRegisteredClaimNames.Iss, _options.ClaimsIssuer);
            }

            yield return new Claim(JwtRegisteredClaimNames.Aud, _options.Audience);

            foreach (var claim in BaselineClaims)
            {
                yield return claim;
            }

            foreach (var claim in claims)
            {
                yield return claim;
            }
        }

        public string BuildJwtString(params Claim[] claims)
        {
            var token = BuildToken(claims);
            return new JwtSecurityTokenHandler().WriteToken(token); 
        }
    }
}