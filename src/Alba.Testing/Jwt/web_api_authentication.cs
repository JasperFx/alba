using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using Alba.Jwt;
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
            var hostBuilder = WebAppSecuredWithJwt.Program.CreateHostBuilder(new string[0]);
                ;
            theSystem = new AlbaHost(hostBuilder, new StubJwt());
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
                // override the StubJwt behavior
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

            var response = await theSystem.Scenario(x =>
            {
                x.Post.Json(input).ToUrl("/math");
                x.StatusCodeShouldBeOk();
            });

            var output = response.ResponseBody.ReadAsJson<Result>();
            output.Sum.ShouldBe(9);
            output.Product.ShouldBe(24);
        }
    }

}