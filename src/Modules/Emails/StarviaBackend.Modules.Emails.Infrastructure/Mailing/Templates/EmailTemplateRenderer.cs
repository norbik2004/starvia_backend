using StarviaBackend.Modules.Emails.Application.Emails.Messages;
using StarviaBackend.Modules.Emails.Application.Emails.Templates;
using StarviaBackend.Modules.Emails.Core.Emails.Enums;

namespace StarviaBackend.Modules.Emails.Infrastructure.Mailing.Templates;

internal sealed class EmailTemplateRenderer(
    IRazorEmailRenderer razorRenderer,
    ConfirmAccountEmail confirmAccountEmail,
    ResetPasswordEmail resetPasswordEmail,
    WelcomingEmail welcomingEmail) : IEmailTemplateRenderer
{
    public async Task<EmailTemplate> RenderAsync(
        EmailType emailType,
        Guid userId,
        string email,
        CancellationToken cancellationToken = default)
    {
        var (subject, body) = emailType switch
        {
            EmailType.WelcomingEmail => (
                WelcomingEmail.Subject,
                await razorRenderer.RenderAsync(
                    "WelcomingEmail/WelcomingEmail.cshtml",
                    welcomingEmail.Create(email))),

            EmailType.ConfirmAccountEmail => (
                ConfirmAccountEmail.Subject,
                await razorRenderer.RenderAsync(
                    "ConfirmAccountEmail/ConfirmAccountEmail.cshtml",
                    await confirmAccountEmail.CreateAsync(userId, email, cancellationToken))),

            EmailType.ResetPasswordEmail => (
                ResetPasswordEmail.Subject,
                await razorRenderer.RenderAsync(
                    "ResetPasswordEmail/ResetPasswordEmail.cshtml",
                    await resetPasswordEmail.CreateAsync(userId, email, cancellationToken))),

            _ => throw new ArgumentOutOfRangeException(
                nameof(emailType),
                emailType,
                "Unsupported email type."),
        };

        var htmlBody = await razorRenderer.RenderAsync(
            "Shared/EmailLayout.cshtml",
            new EmailTemplateContentModel(
                subject,
                body,
                EmailAssets.StarviaLogoDataUri));

        return new EmailTemplate(subject, htmlBody);
    }
}
