using Duende.IdentityServer.Models;

namespace IdentityServer.New
{
    public static class Config
    {

        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };
        public const string ClientId = "spa";
        public const string ApiScope = "api";
        public const string ClientSecret = "secret";

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                  new ApiScope(ApiScope, new[] { "name" }),
            };


        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                // m2m client credentials flow client
                new Client
                {
                    ClientId = ClientId,
                    ClientName = "Client Credentials Client",

                    AllowedGrantTypes = GrantTypes.ResourceOwnerPasswordAndClientCredentials,
                    ClientSecrets = { new Secret(ClientSecret.Sha256()) },

                    AllowedScopes = {  "openid", "profile", ApiScope }
                },
            };
    }
}
