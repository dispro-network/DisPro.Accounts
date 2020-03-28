using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace DisPro.Accounts.Pages.Consent
{
    [SecurityHeaders]
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clientStore;
        private readonly IResourceStore _resourceStore;
        private readonly IEventService _events;
        private readonly ILogger<IndexModel> _logger;

        private bool _showView = false;

        public IndexModel(
            IIdentityServerInteractionService interaction,
            IClientStore clientStore,
            IResourceStore resourceStore,
            IEventService events,
            ILogger<IndexModel> logger)
        {
            _interaction = interaction;
            _clientStore = clientStore;
            _resourceStore = resourceStore;
            _events = events;
            _logger = logger;
        }

        public class InputModel
        {
            public bool RememberConsent { get; set; }
            public string ReturnUrl { get; set; }

        }

        [BindProperty]
        public InputModel Input { get; set; }

        [BindProperty]
        public IEnumerable<string> ScopesConsented { get; set; }


        public string ClientName { get; set; }
        public string ClientUrl { get; set; }
        public string ClientLogoUrl { get; set; }
        public bool AllowRememberConsent { get; set; }

        public List<ScopeViewModel> IdentityScopes { get; set; }
        public List<ScopeViewModel> ResourceScopes { get; set; }



        public async Task OnGetAsync(string returnUrl)
        {
            await BuildInputModelAsync(returnUrl);
        }

        public async Task<IActionResult> OnPostConfirmAsync()
        {
            return await HandleResponse(true);
        }

        public async Task<IActionResult> OnPostDenyAsync()
        {
            return await HandleResponse(false);
        }

        private async Task<IActionResult> HandleResponse(bool confirmed)
        {
            var result = await ProcessConsent(confirmed);

            if (result.IsRedirect)
            {
                // TODO: Update to work with Razor Pages
                //if (await _clientStore.IsPkceClientAsync(result.ClientId))
                //{
                //    // if the client is PKCE then we assume it's native, so this change in how to
                //    // return the response is for better UX for the end user.
                //    return View("Redirect", new RedirectViewModel { RedirectUrl = result.RedirectUri });
                //}

                return Redirect(result.RedirectUri);
            }

            if (result.HasValidationError)
            {
                ModelState.AddModelError(string.Empty, result.ValidationError);
                return Page();
            }

            if (_showView) return Page();

            return RedirectToPage("Error");
        }

        /*****************************************/
        /* helper APIs for the ConsentController */
        /*****************************************/
        private async Task<ProcessConsentResult> ProcessConsent(bool confirmed)
        {
            var result = new ProcessConsentResult();

            // validate return url is still valid
            var request = await _interaction.GetAuthorizationContextAsync(Input.ReturnUrl);
            if (request == null) return result;

            ConsentResponse grantedConsent = null;

            // user clicked 'no' - send back the standard 'access_denied' response
            if (!confirmed)
            {
                grantedConsent = ConsentResponse.Denied;

                // emit event
                await _events.RaiseAsync(new ConsentDeniedEvent(User.GetSubjectId(), request.ClientId, request.ScopesRequested));
            }
            //user clicked 'yes' - validate the data
            else
            {
                // if the user consented to some scope, build the response model
                if (ScopesConsented != null && ScopesConsented.Any())
                {
                    var scopes = ScopesConsented;
                    if (ConsentOptions.EnableOfflineAccess == false)
                    {
                        scopes = scopes.Where(x => x != IdentityServer4.IdentityServerConstants.StandardScopes.OfflineAccess);
                    }

                    grantedConsent = new ConsentResponse
                    {
                        RememberConsent = Input.RememberConsent,
                        ScopesConsented = scopes.ToArray()
                    };

                    // emit event
                    await _events.RaiseAsync(new ConsentGrantedEvent(User.GetSubjectId(), request.ClientId, request.ScopesRequested, grantedConsent.ScopesConsented, grantedConsent.RememberConsent));
                }
                else
                {
                    result.ValidationError = ConsentOptions.MustChooseOneErrorMessage;
                }
            }

            if (grantedConsent != null)
            {
                // communicate outcome of consent back to identityserver
                await _interaction.GrantConsentAsync(request, grantedConsent);

                // indicate that's it ok to redirect back to authorization endpoint
                result.RedirectUri = Input.ReturnUrl;
                result.ClientId = request.ClientId;
            }
            else
            {
                // we need to redisplay the consent UI
                await BuildInputModelAsync(Input.ReturnUrl);
                _showView = true;
            }

            return result;
        }

        private async Task BuildInputModelAsync(string returnUrl)
        {
            var request = await _interaction.GetAuthorizationContextAsync(returnUrl);
            if (request != null)
            {
                var client = await _clientStore.FindEnabledClientByIdAsync(request.ClientId);
                if (client != null)
                {
                    var resources = await _resourceStore.FindEnabledResourcesByScopeAsync(request.ScopesRequested);
                    if (resources != null && (resources.IdentityResources.Any() || resources.ApiResources.Any()))
                    {
                        CreateInputModel(returnUrl, client, resources);
                    }
                    else
                    {
                        _logger.LogError("No scopes matching: {0}", request.ScopesRequested.Aggregate((x, y) => x + ", " + y));
                    }
                }
                else
                {
                    _logger.LogError("Invalid client id: {0}", request.ClientId);
                }
            }
            else
            {
                _logger.LogError("No consent request matching request: {0}", returnUrl);
            }
        }

        private void CreateInputModel(string returnUrl, Client client, Resources resources)
        {
            bool showDefault = false; // if page is loading for the first time, this is used to set default scope checkboxes to checked
            if (Input == null)
            {
                showDefault = true;
                Input = new InputModel
                {
                    RememberConsent = true
                };

            }
            ScopesConsented = ScopesConsented ??= Enumerable.Empty<string>();

            Input.ReturnUrl = returnUrl;

            ClientName = client.ClientName ?? client.ClientId;
            ClientUrl = client.ClientUri;
            ClientLogoUrl = client.LogoUri;
            AllowRememberConsent = client.AllowRememberConsent;


            IdentityScopes = resources.IdentityResources.Select(x => CreateScopeViewModel(x, ScopesConsented.Contains(x.Name) || showDefault)).ToList();
            ResourceScopes = resources.ApiResources.SelectMany(x => x.Scopes).Select(x => CreateScopeViewModel(x, ScopesConsented.Contains(x.Name) || showDefault)).ToList();
            if (ConsentOptions.EnableOfflineAccess && resources.OfflineAccess)
            {
                ResourceScopes = ResourceScopes.Union(new ScopeViewModel[] {
                    GetOfflineAccessScope(ScopesConsented.Contains(IdentityServer4.IdentityServerConstants.StandardScopes.OfflineAccess) || showDefault)
                }).ToList();
            }

        }

        private ScopeViewModel CreateScopeViewModel(IdentityResource identity, bool check)
        {
            return new ScopeViewModel
            {
                Name = identity.Name,
                DisplayName = identity.DisplayName,
                Description = identity.Description,
                Emphasize = identity.Emphasize,
                Required = identity.Required,
                Checked = check || identity.Required
            };
        }

        public ScopeViewModel CreateScopeViewModel(Scope scope, bool check)
        {
            return new ScopeViewModel
            {
                Name = scope.Name,
                DisplayName = scope.DisplayName,
                Description = scope.Description,
                Emphasize = scope.Emphasize,
                Required = scope.Required,
                Checked = check || scope.Required
            };
        }

        private ScopeViewModel GetOfflineAccessScope(bool check)
        {
            return new ScopeViewModel
            {
                Name = IdentityServer4.IdentityServerConstants.StandardScopes.OfflineAccess,
                DisplayName = ConsentOptions.OfflineAccessDisplayName,
                Description = ConsentOptions.OfflineAccessDescription,
                Emphasize = true,
                Checked = check
            };
        }

    }

    public class ProcessConsentResult
    {
        public bool IsRedirect => RedirectUri != null;
        public string RedirectUri { get; set; }
        public string ClientId { get; set; }

        public bool HasValidationError => ValidationError != null;
        public string ValidationError { get; set; }

    }

    public class ConsentOptions
    {
        public static bool EnableOfflineAccess = true;
        public static string OfflineAccessDisplayName = "Offline Access";
        public static string OfflineAccessDescription = "Access to your applications and resources, even when you are offline";

        public static readonly string MustChooseOneErrorMessage = "You must pick at least one permission";
        public static readonly string InvalidSelectionErrorMessage = "Invalid selection";
    }

    public class ScopeViewModel
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool Emphasize { get; set; }
        public bool Required { get; set; }
        public bool Checked { get; set; }
    }
}

