using System;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Alba.Security
{
    public class OpenConnectExtension : IAlbaExtension
    {
        private readonly HttpClient _client;
        private DiscoveryDocumentResponse _disco;
        private JwtBearerOptions _options;

        public OpenConnectExtension()
        {
            _client = new HttpClient();
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }

        public async Task Start(IAlbaHost host)
        {
            // This seems to be necessary to "bake" in the JwtBearerOptions modifications
            var options = host.Services.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
                .Get("Bearer");

            
            _options = options;

            var authorityUrl = options.Authority;
            _disco = await _client.GetDiscoveryDocumentAsync(authorityUrl);
            if (_disco.IsError)
            {
                throw _disco.Exception;
            }
            
        }

        protected virtual Task<TokenResponse> FetchToken(HttpContext context, object tokenCustomization)
        {
            throw new NotImplementedException();
        }
        
        internal async Task ConfigureJwt(HttpContext context)
        {
            //var r = await -_client.Req
            
            var response = await _client.RequestTokenAsync(new PasswordTokenRequest
            {
                Address = _disco.TokenEndpoint,
                ClientId = "",
                ClientSecret = "",
                
            });
            
            // Claim[] claims = extractScenarioSpecificClaims(context);
            // var jwt = BuildJwtString(claims);
            //
            // context.SetBearerToken(jwt);
        }

        public IHostBuilder Configure(IHostBuilder builder)
        {
            throw new System.NotImplementedException();
        }
    }
}