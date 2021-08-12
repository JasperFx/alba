// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.


using System.Collections.Generic;
using Duende.IdentityServer.Models;

namespace IdentityServer
{
    public static class Config
    {
        public static string ClientId;

        static Config()
        {
            ClientId = "spa";
        }

        public const string ApiScope = "api";
        public const string ClientSecret = "secret";

        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope(ApiScope, new[] { "name" }),
            };

        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                new Client
                {
                    ClientId = ClientId,
                    ClientSecrets = { new Secret(ClientSecret.Sha256()) },
                    
                    AllowedGrantTypes =  GrantTypes.ResourceOwnerPasswordAndClientCredentials,

                    RedirectUris = { "https://localhost:5001/signin-oidc" },

                    BackChannelLogoutUri = "https://localhost:5001/bff/backchannel",
                    
                    PostLogoutRedirectUris = { "https://localhost:5001/signout-callback-oidc" },

                    AllowOfflineAccess = true,
                    AllowedScopes = { "openid", "profile", ApiScope }
                },
            };
    }
}