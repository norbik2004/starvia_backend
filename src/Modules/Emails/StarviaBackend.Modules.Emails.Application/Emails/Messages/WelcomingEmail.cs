using StarviaBackend.Modules.Emails.Application.Emails.Links;
using StarviaBackend.Modules.Emails.Application.Emails.Templates.Models;
using StarviaBackend.Shared.Abstractions.App;

namespace StarviaBackend.Modules.Emails.Application.Emails.Messages;

internal sealed class WelcomingEmail(IAppUrls appUrls)
{
    public const string Subject = "Witamy w Starvia!";

    public WelcomingEmailTemplateModel Create(string email) =>
        new(email, AccountEmailLink.Login(appUrls.FrontendBaseUrl));
}
