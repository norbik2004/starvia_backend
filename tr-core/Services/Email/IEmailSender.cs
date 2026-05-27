using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tr_core.Services.Email
{
    public interface IEmailSender
    {
        public Task SendEmailAsync(string to, string subject, string htmlBody);
    }
}
