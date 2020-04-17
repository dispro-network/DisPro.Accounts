using DisPro.Accounts.Data;
using DisPro.Accounts.Migrations.SeedData;
using DisPro.Accounts.Models;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DisPro.Accounts.Migrations
{
    public class Migrator
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly PersistedGrantDbContext _persistedGrantDbContext;
        private readonly ConfigurationDbContext _configurationDbContext;
        private readonly IdentityServerSeedDatacs _identityServerConfig;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly UserSeedData _userSeedData;

        public Migrator(ApplicationDbContext applicationDbContext,
            PersistedGrantDbContext persistedGrantDbContext,
            ConfigurationDbContext configurationDbContext,
            IdentityServerSeedDatacs identityServerConfig,
            UserManager<ApplicationUser> userManager,
            UserSeedData userSeedData)
        {
            _applicationDbContext = applicationDbContext;
            _persistedGrantDbContext = persistedGrantDbContext;
            _configurationDbContext = configurationDbContext;
            _identityServerConfig = identityServerConfig;
            _userManager = userManager;
            _userSeedData = userSeedData;
        }

        public void Migrate()
        {
            _applicationDbContext.Database.Migrate();
            _persistedGrantDbContext.Database.Migrate();
            _configurationDbContext.Database.Migrate();
        }

        public void Destroy()
        {
            _applicationDbContext.Database.EnsureDeleted();
            _persistedGrantDbContext.Database.EnsureDeleted();
            _configurationDbContext.Database.EnsureDeleted();
        }


        public void SeedIdentityServer()
        {
            if (!_configurationDbContext.Clients.Any())
            {
                foreach (var client in _identityServerConfig.GetClients())
                {
                    _configurationDbContext.Clients.Add(client.ToEntity());
                }
                _configurationDbContext.SaveChanges();
            }

            if (!_configurationDbContext.IdentityResources.Any())
            {
                foreach (var resource in _identityServerConfig.GetIdentityResources())
                {
                    _configurationDbContext.IdentityResources.Add(resource.ToEntity());
                }
                _configurationDbContext.SaveChanges();
            }

            if (!_configurationDbContext.ApiResources.Any())
            {
                foreach (var resource in _identityServerConfig.GetApis())
                {
                    _configurationDbContext.ApiResources.Add(resource.ToEntity());
                }
                _configurationDbContext.SaveChanges();
            }
        }

        public void SeedUsers()
        {
            foreach (var userKvp in _userSeedData.GetUsers())
            {
                var identityResult = _userManager.CreateAsync(userKvp.Key, "Pass123$").Result;
                if (!identityResult.Succeeded)
                {
                    throw new Exception(identityResult.Errors.First().Description);
                }
                var user = _userManager.FindByNameAsync(userKvp.Key.UserName).Result;
                identityResult = _userManager.AddClaimsAsync(user, userKvp.Value).Result;
                if (!identityResult.Succeeded)
                {
                    throw new Exception(identityResult.Errors.First().Description);
                }
            }
        }
    }
}
