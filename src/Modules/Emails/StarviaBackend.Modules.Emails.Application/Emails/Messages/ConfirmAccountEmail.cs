using StarviaBackend.Modules.Accounts.Application.Users.Commands.GenerateEmailConfirmationToken;
using StarviaBackend.Modules.Emails.Application.Emails.Links;
using StarviaBackend.Modules.Emails.Application.Emails.Templates.Models;
using StarviaBackend.Shared.Abstractions.App;
using StarviaBackend.Shared.Abstractions.Dispatchers;

namespace StarviaBackend.Modules.Emails.Application.Emails.Messages;

internal sealed class ConfirmAccountEmail(IDispatcher dispatcher, IAppUrls appUrls)
{
    public const string Subject = "Potwierdź swoje konto w Starvia";

    public async Task<ConfirmAccountEmailTemplateModel> CreateAsync(
        Guid userId,
        string email,
        CancellationToken cancellationToken)
    {
        var code = await dispatcher.SendAsync<GenerateEmailConfirmationTokenCommand, GenerateEmailConfirmationTokenResult>(
            new GenerateEmailConfirmationTokenCommand(userId),
            cancellationToken);

        var link = AccountEmailLink.ConfirmAccount(appUrls.FrontendBaseUrl, userId, code.Token);
        return new ConfirmAccountEmailTemplateModel(email, link);
    }
}
