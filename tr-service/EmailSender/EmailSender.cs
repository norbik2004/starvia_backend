using Stripe.Tax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tr_core.Services.Email;
using MimeKit;

namespace tr_service.EmailSender
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            var emailMessage = new MimeMessage();
            emailMessage.From.Add(new MailboxAddress(settings.Value.Name, settings.Value.EmailId));
            emailMessage.To.Add(new MailboxAddress(to, to.TrimEnd()));
            emailMessage.Subject = subject;
        }
    }
}
