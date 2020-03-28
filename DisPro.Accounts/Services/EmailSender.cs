using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DisPro.Accounts.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Task.CompletedTask;
        }
    }

}
