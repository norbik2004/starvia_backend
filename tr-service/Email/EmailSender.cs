using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using tr_core.Services.Email;

namespace tr_service.Email
{
    public class EmailSender(IOptions<MailSettings> settings, ILogger<EmailSender> logger) : IEmailSender
    {
        public async Task SendEmailAsync(string to, string subject, string htmlBody)
        {
            try
            {
                var emailMessage = new MimeMessage();
                emailMessage.From.Add(new MailboxAddress(settings.Value.Name, settings.Value.EmailId));
                emailMessage.To.Add(new MailboxAddress(to, to.TrimEnd()));
                emailMessage.Subject = subject;

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = htmlBody,
                    TextBody = "Please view this email in HTML format."
                };


                emailMessage.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(settings.Value.Host, settings.Value.Port, settings.Value.UseSSL);
                if (!settings.Value.DefaultCredentials) await client.AuthenticateAsync(settings.Value.EmailId, settings.Value.Password);

                await client.SendAsync(emailMessage);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while sending email");
            }

        }
    }
}
