using MassTransit;
using StarviaBackend.Modules.Accounts.Core.Users.Repositories;
using StarviaBackend.Shared.Abstractions.Commands;
using StarviaBackend.Shared.Abstractions.Email;

namespace StarviaBackend.Modules.Accounts.Application.Users.Commands.RequestPasswordReset;

internal sealed class RequestPasswordResetHandler(
    IUsersRepository usersRepository,
    IPublishEndpoint publishEndpoint)
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

        await publishEndpoint.Publish(
            new SendEmailRequestedEvent(
                user.Id,
                command.Email,
                ResetPasswordEmailType),
            cancellationToken);
    }
}
