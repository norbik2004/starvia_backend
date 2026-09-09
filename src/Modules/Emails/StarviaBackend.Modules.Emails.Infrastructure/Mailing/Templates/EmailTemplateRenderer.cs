using StarviaBackend.Modules.Emails.Application.Emails.Templates;
using StarviaBackend.Modules.Emails.Application.Emails.Templates.Models;
using StarviaBackend.Modules.Emails.Core.Emails.Enums;

namespace StarviaBackend.Modules.Emails.Infrastructure.Mailing.Templates;

internal sealed class EmailTemplateRenderer(
    IRazorEmailRenderer razorRenderer) : IEmailTemplateRenderer
{
    public async Task<EmailTemplate> RenderAsync(
     EmailType emailType,
     EmailTemplateModel model)
    {
        string subject;
        string body;

        switch (emailType)
        {
            case EmailType.WelcomingEmail:
                subject = "Witamy w Starvia!";

                body = await razorRenderer.RenderAsync(
                    "WelcomingEmail/WelcomingEmail.cshtml",
                    new WelcomingEmailTemplateModel(
                        model.Email,
                        model.Link!));

                break;

            case EmailType.ConfirmAccountEmail:
                subject = "Potwierdz swoje konto w Starvia";

                body = await razorRenderer.RenderAsync(
                    "ConfirmAccountEmail/ConfirmAccountEmail.cshtml",
                    new ConfirmAccountEmailTemplateModel(
                        model.Email,
                        model.UserName!,
                        model.Link!));

                break;

            case EmailType.ResetPasswordEmail:
                subject = "Zresetuj hasło swojego konta w Starvia";

                body = await razorRenderer.RenderAsync(
                    "ResetPasswordEmail/ResetPasswordEmail.cshtml",
                    new ResetPasswordEmailTemplateModel(
                        model.Email,
                        model.UserName!,
                        model.Code!));

                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(emailType),
                    emailType,
                    "Unsupported email type.");
        }

        var htmlBody = await razorRenderer.RenderAsync(
            "Shared/EmailLayout.cshtml",
            new EmailTemplateContentModel(
                subject,
                body));

        return new EmailTemplate(
            subject,
            htmlBody);
    }
}
