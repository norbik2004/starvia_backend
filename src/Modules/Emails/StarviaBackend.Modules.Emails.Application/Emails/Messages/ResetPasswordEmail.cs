using StarviaBackend.Modules.Accounts.Application.Users.Commands.GeneratePasswordResetToken;
using StarviaBackend.Modules.Emails.Application.Emails.Links;
using StarviaBackend.Modules.Emails.Application.Emails.Templates.Models;
using StarviaBackend.Shared.Abstractions.App;
using StarviaBackend.Shared.Abstractions.Dispatchers;

namespace StarviaBackend.Modules.Emails.Application.Emails.Messages;

internal sealed class ResetPasswordEmail(IDispatcher dispatcher, IAppUrls appUrls)
{
    public const string Subject = "Zresetuj hasło swojego konta w Starvia";

    public async Task<ResetPasswordEmailTemplateModel> CreateAsync(
        Guid userId,
        string email,
        CancellationToken cancellationToken)
    {
        var code = await dispatcher.SendAsync<GeneratePasswordResetTokenCommand, GeneratePasswordResetTokenResult>(
            new GeneratePasswordResetTokenCommand(userId),
            cancellationToken);

        var link = AccountEmailLink.ResetPassword(appUrls.FrontendBaseUrl, userId, code.Token);
        return new ResetPasswordEmailTemplateModel(email, link);
    }
}
