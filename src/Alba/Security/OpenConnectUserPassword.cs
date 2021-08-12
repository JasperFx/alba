using System;
using System.Net.Http;
using System.Threading.Tasks;
using Baseline;
using IdentityModel.Client;

namespace Alba.Security
{
    public class OpenConnectUserPassword : OpenConnectExtension
    {
        public override void AssertValid()
        {
            if (ClientId.IsEmpty()) throw new Exception($"{nameof(ClientId)} cannot be null");
            if (ClientSecret.IsEmpty()) throw new Exception($"{nameof(ClientSecret)} cannot be null");
            if (UserName.IsEmpty()) throw new Exception($"{nameof(UserName)} cannot be null");
            if (Password.IsEmpty()) throw new Exception($"{nameof(Password)} cannot be null");
        }
        
        public string UserName { get; set; }
        public string Password { get; set; }

        public override Task<TokenResponse> FetchToken(HttpClient client, DiscoveryDocumentResponse disco, object? tokenCustomization)
        {
            return client.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = disco.TokenEndpoint,
                ClientId = ClientId,
                ClientSecret = ClientSecret,
                UserName = UserName,
                Password = Password
            });
        }
    }
}