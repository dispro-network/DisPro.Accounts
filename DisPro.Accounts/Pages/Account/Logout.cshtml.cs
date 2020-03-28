using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using DisPro.Accounts.Models;

namespace DisPro.Accounts.Pages.Account
{
    [AllowAnonymous]
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IEventService _events;
        private readonly ILogger<LogoutModel> _logger;
        private readonly IPersistedGrantStore _persistedGrantStore;

        public LogoutModel(SignInManager<ApplicationUser> signInManager,
            IIdentityServerInteractionService interaction,
            IEventService events,
            ILogger<LogoutModel> logger,
            IPersistedGrantStore persistedGrantStore)
        {
            _signInManager = signInManager;
            _interaction = interaction;
            _events = events;
            _logger = logger;
            _persistedGrantStore = persistedGrantStore;
        }

        public string PostLogoutRedirectUri { get; set; }
        public string ClientName { get; set; }
        public string SignOutIframeUrl { get; set; }

        public bool AutomaticRedirectAfterSignOut
        {
            get; set;
        }

        public async Task<IActionResult> OnGetAsync(string logoutId = null)
        {
            var context = await _interaction.GetLogoutContextAsync(logoutId);
            if (logoutId != null && context != null && context.ShowSignoutPrompt == false)
            {
                return await OnPostAsync(logoutId);
            }

            return LocalRedirect("/");
        }

        public async Task<IActionResult> OnPostAsync(string logoutId = null, string returnUrl = null)
        {

            if (logoutId == null) logoutId = await _interaction.CreateLogoutContextAsync();

            LogoutRequest logoutContext = await _interaction.GetLogoutContextAsync(logoutId);

            AutomaticRedirectAfterSignOut = true;
            PostLogoutRedirectUri = logoutContext?.PostLogoutRedirectUri;
            ClientName = string.IsNullOrEmpty(logoutContext?.ClientName) ? logoutContext?.ClientId : logoutContext?.ClientName;
            SignOutIframeUrl = logoutContext?.SignOutIFrameUrl;

            // delete the local authentication cookie
            await _signInManager.SignOutAsync();

            try
            {
                // delete reference tokens
                var subjectId = HttpContext.User.Identity.GetSubjectId();
                await _persistedGrantStore.RemoveAllAsync(subjectId, "mvc_hybrid", "reference_token");
                await _persistedGrantStore.RemoveAllAsync(subjectId, "js", "reference_token");
                await _persistedGrantStore.RemoveAllAsync(subjectId, "react", "reference_token");

                // raise the logout event
                await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
                _logger.LogInformation("User logged out.");

                // set this so UI rendering sees an anonymous user
                HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());
            }
            catch(InvalidOperationException ioe)
            {
                if (ioe.Message == "sub claim is missing") _logger.LogWarning("Logged out user attempting to log out");
                else throw;
            }
            catch (Exception e)
            {
                _logger.LogError("Error while attempting to log user out.", e);
            }


            return Page();
        }
    }
}
