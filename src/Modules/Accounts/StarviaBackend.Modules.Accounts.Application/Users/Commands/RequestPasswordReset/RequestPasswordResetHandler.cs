using MassTransit;
using StarviaBackend.Modules.Accounts.Application.Users.Commands.GeneratePasswordResetToken;
using StarviaBackend.Modules.Accounts.Application.Users.Commands.ResetPassword;
using StarviaBackend.Modules.Accounts.Core.Users.Repositories;
using StarviaBackend.Shared.Abstractions.App;
using StarviaBackend.Shared.Abstractions.Commands;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Email;

namespace StarviaBackend.Modules.Accounts.Application.Users.Commands.RequestPasswordReset;

internal sealed class RequestPasswordResetHandler(
    IUsersRepository usersRepository,
    IDispatcher dispatcher,
    IPublishEndpoint publishEndpoint,
    IAppUrls appUrls)
    : ICommandHandler<RequestPasswordResetCommand>
{
    private const string ResetPasswordEmailType = "ResetPasswordEmail";

    public async Task HandleAsync(RequestPasswordResetCommand command, CancellationToken cancellationToken = default)
    {
        var user = await usersRepository.GetByEmailAsync(command.Email, cancellationToken);
        if (user is null)
        {
            return;
        }

        var token = await dispatcher.SendAsync<GeneratePasswordResetTokenCommand, GeneratePasswordResetTokenResult>(
            new GeneratePasswordResetTokenCommand(user.Id),
            cancellationToken);

        var resetLink = PasswordResetLink.Build(appUrls.FrontendBaseUrl, user.Id, token.Token);

        await publishEndpoint.Publish(
            new SendEmailRequestedEvent(
                user.Id,
                command.Email,
                ResetPasswordEmailType,
                Code: token.Token,
                Link: resetLink),
            cancellationToken);
    }
}
