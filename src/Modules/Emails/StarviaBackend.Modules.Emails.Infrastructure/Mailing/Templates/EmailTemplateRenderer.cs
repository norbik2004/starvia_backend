using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Emails.Application.Emails.Messages;
using StarviaBackend.Modules.Emails.Application.Emails.Templates;
using StarviaBackend.Shared.Abstractions.Email;

namespace StarviaBackend.Modules.Emails.Infrastructure.Mailing.Templates;

internal sealed class EmailTemplateRenderer(
    IRazorEmailRenderer razorRenderer,
    IServiceProvider serviceProvider) : IEmailTemplateRenderer
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
                    serviceProvider.GetRequiredService<WelcomingEmail>().Create(email))),

            EmailType.ConfirmAccountEmail => (
                ConfirmAccountEmail.Subject,
                await razorRenderer.RenderAsync(
                    "ConfirmAccountEmail/ConfirmAccountEmail.cshtml",
                    await serviceProvider
                        .GetRequiredService<ConfirmAccountEmail>()
                        .CreateAsync(userId, email, cancellationToken))),

            EmailType.ResetPasswordEmail => (
                ResetPasswordEmail.Subject,
                await razorRenderer.RenderAsync(
                    "ResetPasswordEmail/ResetPasswordEmail.cshtml",
                    await serviceProvider
                        .GetRequiredService<ResetPasswordEmail>()
                        .CreateAsync(userId, email, cancellationToken))),

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
