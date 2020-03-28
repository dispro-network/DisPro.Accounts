using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DisPro.Accounts.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public class ErrorModel : PageModel
    {
        private readonly ILogger<ErrorModel> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly IIdentityServerInteractionService _interaction;
        
        public ErrorModel(ILogger<ErrorModel> logger, IIdentityServerInteractionService interaction, IWebHostEnvironment environment)
        {
            _logger = logger;
            _interaction = interaction;
            _environment = environment;
        }
        public ErrorMessage Error { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(Error?.RequestId);


        public async Task<IActionResult> OnGetAsync(string errorId)
        {
            Error = await _interaction.GetErrorContextAsync(errorId);
            if(Error!= null)
            {
                if (!_environment.IsDevelopment())
                {
                    // only show in development
                    Error.ErrorDescription = null;
                }
            }
            else
            {
                Error.RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            }
            _logger.LogError("Error page was reached.\nerrorId: {0}\nRequestId: {1}\nError: {2}\nErrorDescription: {3}",
                errorId, Error.RequestId,Error.Error, Error.ErrorDescription);
            return Page();
        }
    }
}
