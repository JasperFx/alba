using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace Alba.Jwt
{
    public class StubJwt : IAlbaExtension, IHasClaims, IPostConfigureOptions<JwtBearerOptions>
    {
        private SecurityKey _signingKey;
        private readonly IList<Claim> _baselineClaims = new List<Claim>();
        private JwtBearerOptions _options;


        public void Dispose()
        {
            // Nothing
        }

        public ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }

        public Task Start(IAlbaHost host)
        {
            // This seems to be necessary to "bake" in the JwtBearerOptions modifications
            var options = host.Services.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
                .Get("Bearer");


            host.BeforeEach(ConfigureJwt);
            return Task.CompletedTask;
        }

        // TODO -- need to set custom claims on the HttpContext
        internal void ConfigureJwt(HttpContext context)
        {
            var jwt = BuildJwtString();

            // TODO -- add a helper on Scenario and on HttpContext
            context.Request.Headers["Authorization"] = $"Bearer {jwt}";
        }

        public IHostBuilder Configure(IHostBuilder builder)
        {
            return builder.ConfigureServices(services =>
            {
                services.AddSingleton<IPostConfigureOptions<JwtBearerOptions>>(this);
            });
        }

        void IHasClaims.AddClaim(Claim claim)
        {
            _baselineClaims.Add(claim);
        }
        
        internal JwtSecurityToken BuildToken(params Claim[] claims)
        {
            // TODO -- get the algorithm from the options validation parameters,
            // but use HmacSha256 as the default
            var credentials = new SigningCredentials(_options.TokenValidationParameters.IssuerSigningKey,
                SecurityAlgorithms.HmacSha256);

            return new JwtSecurityToken(_options.ClaimsIssuer, _options.Audience, allClaims(claims),
                expires: DateTime.UtcNow.AddDays(1), signingCredentials: credentials);
        }
        
        internal string BuildJwtString(params Claim[] claims)
        {
            var token = BuildToken(claims);
            return new JwtSecurityTokenHandler().WriteToken(token); 
        }
        
        public IEnumerable<Claim> allClaims(Claim[] claims)
        {
            yield return new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString());
            if (_options.ClaimsIssuer != null)
            {
                yield return new Claim(JwtRegisteredClaimNames.Iss, _options.ClaimsIssuer);
            }

            yield return new Claim(JwtRegisteredClaimNames.Aud, _options.Audience);

            foreach (var claim in _baselineClaims)
            {
                yield return claim;
            }

            foreach (var claim in claims)
            {
                yield return claim;
            }
        }

        void IPostConfigureOptions<JwtBearerOptions>.PostConfigure(string name, JwtBearerOptions options)
        {
            // This will deactivate the callout to the OIDC server
            options.ConfigurationManager =
                new StaticConfigurationManager<OpenIdConnectConfiguration>(new OpenIdConnectConfiguration
                {
                    
                });
            
            var original = options.TokenValidationParameters;
            _signingKey = original.IssuerSigningKey 
                          ?? new SymmetricSecurityKey(Encoding.UTF8.GetBytes("some really big key that should work"));

            options.TokenValidationParameters = new TokenValidationParameters
            {
                IssuerSigningKey = _signingKey,
                ValidIssuer = original.ValidIssuer,
                ValidAudience = original.ValidAudience,
                ValidateIssuer = false
            };
            
            options.Authority = null;
            options.MetadataAddress = null;

            _options = options;
        }
    }

    public interface IHasClaims
    {
        void AddClaim(Claim claim);
    }
    
    public static class ClaimsExtensions
    {
        public static T With<T>(this T claims, Claim claim) where T : IHasClaims
        {
            claims.AddClaim(claim);
            return claims;
        }
        
        public static T With<T>(this T claims, string type, string value) where T : IHasClaims
        {
            var claim = new Claim(type, value);
            return claims.With(claim);
        }
    }
}