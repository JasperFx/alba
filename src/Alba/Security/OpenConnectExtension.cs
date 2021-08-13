using System;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

#nullable enable

namespace Alba.Security
{
    public abstract class OpenConnectExtension : IAlbaExtension
    {
        internal static readonly string OverrideKey = "alba_oidc_override";
        
        private readonly HttpClient _client;
        private DiscoveryDocumentResponse? _disco;
        private TokenResponse? _cached;

        public static void StoreCustomization(HttpContext context, object customization)
        {
            context.Items.Add(OverrideKey, customization);
        }

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
            AssertValid();
            
            // This seems to be necessary to "bake" in the JwtBearerOptions modifications
            var options = host.Services.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>()
                .Get("Bearer");
            
            var authorityUrl = options.Authority;
            _disco = await _client.GetDiscoveryDocumentAsync(authorityUrl);
            if (_disco.IsError)
            {
                throw _disco.Exception;
            }

            host.BeforeEachAsync(ConfigureJwt);

        }
        
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }

        /// <summary>
        /// Validate that all the necessary information like ClientSecret and ClientId have been
        /// supplied to this extension
        /// </summary>
        public abstract void AssertValid();

        public Task<TokenResponse> FetchToken(object? tokenCustomization)
        {
            if (_disco == null)
                throw new InvalidOperationException(
                    "This operation is not possible without an existing OIDC discovery document");
            return FetchToken(_client, _disco, tokenCustomization);
        }

        public abstract Task<TokenResponse> FetchToken(HttpClient client, DiscoveryDocumentResponse? disco,
            object? tokenCustomization);

        private async Task<TokenResponse> determineJwt(HttpContext context)
        {
            if (context.Items.TryGetValue(OverrideKey, out var scenarioOverride))
            {
                return await FetchToken(_client, _disco, scenarioOverride);
            }

            _cached ??= await FetchToken(_client, _disco, null);

            return _cached;
        }
        
        internal async Task ConfigureJwt(HttpContext context)
        {
            var token = await determineJwt(context);
            context.SetBearerToken(token.AccessToken);
        }

        public IHostBuilder Configure(IHostBuilder builder)
        {
            return builder;
        }
    }
}