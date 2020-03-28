using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace DisPro.Accounts
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };
        }

        public static IEnumerable<ApiResource> GetApis()
        {
            return new List<ApiResource>
            {
                new ApiResource
                {
                    Name="default-api",
                    DisplayName="Default API",
                    Scopes = { new Scope("default-scope", "Default Scope")},
                    ApiSecrets = {new Secret("secret".Sha256())}
                }
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "dispro-webclient",
                    ClientName = "DisPro Web Client",
                    AllowedGrantTypes = GrantTypes.Code,
                    AccessTokenType = AccessTokenType.Reference,
                    RequirePkce = true,
                    RequireClientSecret = false,
                    RequireConsent = false,

                    RedirectUris =           { "https://dispro.network.local:5005/callback" },
                    PostLogoutRedirectUris = { "https://dispro.network.local:5005/" },
                    AllowedCorsOrigins =     { "https://dispro.network.local:5005" },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "default-scope"
                    }
                }
            };
        }
    }
}