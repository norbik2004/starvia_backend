using StarviaBackend.Shared.Abstractions.Email;

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
