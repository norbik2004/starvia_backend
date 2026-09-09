using StarviaBackend.Modules.Emails.Application.Emails.Templates;
using StarviaBackend.Modules.Emails.Application.Emails.Templates.Models;
using StarviaBackend.Modules.Emails.Core.Emails.Enums;

namespace StarviaBackend.Modules.Emails.Infrastructure.Mailing.Templates;

internal sealed class EmailTemplateRenderer(
    IRazorEmailRenderer razorRenderer) : IEmailTemplateRenderer
{
    private const string DefaultAppLink = "https://starvia.pl/login";

    public async Task<EmailTemplate> RenderAsync(
        EmailType emailType,
        EmailTemplateModel model)
    {
        var displayName = string.IsNullOrWhiteSpace(model.UserName)
            ? model.Email
            : model.UserName;

        var (subject, body) = emailType switch
        {
            EmailType.WelcomingEmail => (
                "Witamy w Starvia!",
                await razorRenderer.RenderAsync(
                    "WelcomingEmail/WelcomingEmail.cshtml",
                    new WelcomingEmailTemplateModel(
                        displayName,
                        model.Link ?? DefaultAppLink))),

            EmailType.ConfirmAccountEmail => (
                "Potwierdź swoje konto w Starvia",
                await razorRenderer.RenderAsync(
                    "ConfirmAccountEmail/ConfirmAccountEmail.cshtml",
                    new ConfirmAccountEmailTemplateModel(
                        displayName,
                        Required(model.Link, nameof(model.Link), emailType)))),

            EmailType.ResetPasswordEmail => (
                "Zresetuj hasło swojego konta w Starvia",
                await razorRenderer.RenderAsync(
                    "ResetPasswordEmail/ResetPasswordEmail.cshtml",
                    new ResetPasswordEmailTemplateModel(
                        displayName,
                        Required(model.Link, nameof(model.Link), emailType)))),

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

    private static string Required(string? value, string name, EmailType emailType) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException(
                $"{name} is required for {emailType}.",
                name)
            : value;
}
