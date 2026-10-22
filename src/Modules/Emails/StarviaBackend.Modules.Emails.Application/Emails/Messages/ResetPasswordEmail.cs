using StarviaBackend.Modules.Emails.Application.Emails.Links;
using StarviaBackend.Modules.Emails.Application.Emails.Templates.Models;
using StarviaBackend.Modules.Emails.Application.Emails.Tokens;
using StarviaBackend.Shared.Abstractions.App;

namespace StarviaBackend.Modules.Emails.Application.Emails.Messages;

internal sealed class ResetPasswordEmail(IAccountTokenProvider tokenProvider, IAppUrls appUrls)
{
    public const string Subject = "Zresetuj hasło swojego konta w Starvia";

    public async Task<ResetPasswordEmailTemplateModel> CreateAsync(
        Guid userId,
        string email,
        CancellationToken cancellationToken)
    {
        var token = await tokenProvider.GetPasswordResetTokenAsync(userId, cancellationToken);

        var link = AccountEmailLink.ResetPassword(appUrls.FrontendBaseUrl, userId, token);
        return new ResetPasswordEmailTemplateModel(email, link);
    }
}
