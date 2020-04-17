using DisPro.Accounts.Models;
using IdentityModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DisPro.Accounts.Migrations.SeedData
{
    public class UserSeedData
    {
        public IDictionary<ApplicationUser, IEnumerable<Claim>> GetUsers()
        {
            var users = new Dictionary<ApplicationUser, IEnumerable<Claim>>();

            string aliceEmail = "alicesmith@email.com";
            var alice = new ApplicationUser
            {
                UserName = aliceEmail,
                Email = aliceEmail,
                EmailConfirmed = true
            };
            var aliceClaims = new Claim[] {
                new Claim(JwtClaimTypes.Name, "Alice Smith"),
                new Claim(JwtClaimTypes.GivenName, "Alice"),
                new Claim(JwtClaimTypes.FamilyName, "Smith"),
                new Claim(JwtClaimTypes.Email, alice.Email),
                new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                new Claim(JwtClaimTypes.WebSite, "http://alice.com"),
                new Claim(JwtClaimTypes.Address, @"{ 'street_address': 'One Hacker Way', 'locality': 'Heidelberg', 'postal_code': 69118, 'country': 'Germany' }", IdentityServer4.IdentityServerConstants.ClaimValueTypes.Json)};
            users.Add(alice, aliceClaims);

            string bobEmail = "bobsmith@email.com";
            var bob = new ApplicationUser
            {
                UserName = bobEmail,
                Email = bobEmail,
                EmailConfirmed = true
            };
            var bobClaims = new Claim[] {
                new Claim(JwtClaimTypes.Name, "Bob Smith"),
                new Claim(JwtClaimTypes.GivenName, "Bob"),
                new Claim(JwtClaimTypes.FamilyName, "Smith"),
                new Claim(JwtClaimTypes.Email, bob.Email)};
            users.Add(bob, bobClaims);

            return users;
        }
    }
}
