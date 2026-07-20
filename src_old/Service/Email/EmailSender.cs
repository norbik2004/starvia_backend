using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Core.Application.Services.Email;
using Core.Application.DTO.Email.Models;
using Service.Email.Forms;
using Core.Application;

namespace Service.Email
{
    public class EmailSender(IOptions<MailSettings> settings, ILogger<EmailSender> logger, IOptions<ApplicationSettings> appSettings) : IEmailSender
    {
        public async Task<bool> SendConfirmationEmail(ConfirmEmailRequest request)
        {
            try
            {
                var htmlBody = ConfirmEmailTemplate.GenerateUserMessageTemplate(request, appSettings.Value.BackendURL);

                await SendEmailAsync(request.To, request.Subject, htmlBody);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while sending confirmation email");
                return false;
            }
        }

        public async Task<bool> SendPasswordResetEmail(PasswordResetEmailRequest request)
        {
            try
            {
                var htmlBody = ResetPasswordTemplate.GenerateUserMessageTemplate(request, appSettings.Value.FrontendURL);

                await SendEmailAsync(request.To, request.Subject, htmlBody);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while sending confirmation email");
                return false;
            }
        }

        private async Task SendEmailAsync(string to, string subject, string htmlBody)
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
