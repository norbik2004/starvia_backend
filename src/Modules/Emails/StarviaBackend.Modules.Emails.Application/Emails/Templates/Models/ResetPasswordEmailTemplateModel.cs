namespace StarviaBackend.Modules.Emails.Application.Emails.Templates.Models;

public sealed record ResetPasswordEmailTemplateModel(string UserName, string Link);
