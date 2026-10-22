using StarviaBackend.Modules.Emails.Application.Emails.Links;
using StarviaBackend.Modules.Emails.Application.Emails.Templates.Models;
using StarviaBackend.Modules.Emails.Application.Emails.Tokens;
using StarviaBackend.Shared.Abstractions.App;

namespace StarviaBackend.Modules.Emails.Application.Emails.Messages;

internal sealed class ConfirmAccountEmail(IAccountTokenProvider tokenProvider, IAppUrls appUrls)
{
    public const string Subject = "Potwierdź swoje konto w Starvia";

    public async Task<ConfirmAccountEmailTemplateModel> CreateAsync(
        Guid userId,
        string email,
        CancellationToken cancellationToken)
    {
        var token = await tokenProvider.GetEmailConfirmationTokenAsync(userId, cancellationToken);

        var link = AccountEmailLink.ConfirmAccount(appUrls.FrontendBaseUrl, userId, token);
        return new ConfirmAccountEmailTemplateModel(email, link);
    }
}
