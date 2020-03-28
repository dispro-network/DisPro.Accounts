using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DisPro.Accounts.Pages.Account
{
    [AllowAnonymous]
    public class LockoutModel : PageModel
    {
        public void OnGet()
        {

        }
    }
}
