using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;

namespace DisPro.Accounts.Migrations.SeedData
{
    public class IdentityServerSeedDatacs
    {
        private readonly IConfiguration _config;
        public IdentityServerSeedDatacs(IConfiguration config)
        {
            _config = config;
        }

        public IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };
        }

        public IEnumerable<ApiResource> GetApis()
        {
            var secretConfigBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("secrets/appsettings.secrets.json", optional: false);
            var secretConfig = secretConfigBuilder.Build();
            var apiSecrectsSection = secretConfig.GetSection("IdentityServer").GetSection("ApiSecrets");

            return new List<ApiResource>
            {
                new ApiResource
                {
                    Name="default-api",
                    DisplayName="Default API",
                    Scopes = { new Scope("default-scope", "Default Scope")},
                    ApiSecrets = {new Secret(apiSecrectsSection.GetValue<string>("default-api").Sha256())}
                }
            };
        }

        public IEnumerable<Client> GetClients()
        {
            var baseUrl = _config.GetValue<string>("BaseUrl");
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

                    RedirectUris =           { baseUrl + "/callback" },
                    PostLogoutRedirectUris = { baseUrl + "/" },
                    AllowedCorsOrigins =     { baseUrl },

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
