namespace StarviaBackend.Modules.Emails.Application.Emails.Templates.Models;

public sealed record ConfirmAccountEmailTemplateModel(string UserName, string Link);
