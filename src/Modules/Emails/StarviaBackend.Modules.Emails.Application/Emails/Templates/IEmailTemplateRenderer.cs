using StarviaBackend.Modules.Emails.Core.Emails.Enums;

namespace StarviaBackend.Modules.Emails.Application.Emails.Templates;

internal interface IEmailTemplateRenderer
{
    EmailTemplate Render(EmailType emailType, EmailTemplateModel model);
}

internal sealed record EmailTemplate(string Subject, string HtmlBody);

internal sealed record EmailTemplateModel(string Email, string? Code = null, string? UserName = null);
