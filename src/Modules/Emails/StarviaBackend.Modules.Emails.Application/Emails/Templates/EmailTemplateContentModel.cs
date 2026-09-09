namespace StarviaBackend.Modules.Emails.Application.Emails.Templates;

public sealed record EmailTemplateContentModel(
    string Title,
    string Body,
    string LogoSrc);
