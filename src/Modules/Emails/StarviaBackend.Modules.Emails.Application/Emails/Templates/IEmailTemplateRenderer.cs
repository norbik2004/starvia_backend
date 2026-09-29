using StarviaBackend.Modules.Emails.Core.Emails.Enums;

namespace StarviaBackend.Modules.Emails.Application.Emails.Templates;

internal interface IEmailTemplateRenderer
{
    Task<EmailTemplate> RenderAsync(
        EmailType emailType,
        Guid userId,
        string email,
        CancellationToken cancellationToken = default);
}

internal sealed record EmailTemplate(
    string Subject,
    string HtmlBody);
