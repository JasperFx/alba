using System;
using System.Net.Http;
using System.Threading.Tasks;
using Baseline;
using IdentityModel.Client;
using Microsoft.AspNetCore.Http;

namespace Alba.Security
{
    /// <summary>
    /// Apply OIDC security workflow to outgoing Alba scenario requests using the
    /// Client Credentials workflow
    /// </summary>
    public class OpenConnectClientCredentials : OpenConnectExtension
    {
        public override void AssertValid()
        {
            if (ClientId.IsEmpty()) throw new Exception($"{nameof(ClientId)} cannot be null");
            if (ClientSecret.IsEmpty()) throw new Exception($"{nameof(ClientSecret)} cannot be null");
            if (Scope.IsEmpty()) throw new Exception($"{nameof(Scope)} cannot be null");
        }
        
        public string? Scope { get; set; }

        public override Task<TokenResponse> FetchToken(HttpClient client, DiscoveryDocumentResponse? disco,
            object? tokenCustomization)
        {
            if (disco == null) throw new ArgumentNullException(nameof(disco), "Unable to load the token discovery document");

            return client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = ClientId,
                ClientSecret = ClientSecret,
                Scope = Scope
            });
        }
    }
}