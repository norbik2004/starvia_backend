using MassTransit;
using Microsoft.AspNetCore.Identity;
using StarviaBackend.Modules.Accounts.Core.Users.Entities;
using StarviaBackend.Modules.Accounts.Core.Users.Exceptions;
using StarviaBackend.Modules.Accounts.Core.Users.Repositories;
using StarviaBackend.Shared.Abstractions.Commands;
using StarviaBackend.Shared.Abstractions.Email;

namespace StarviaBackend.Modules.Accounts.Application.Users.Commands.ConfirmEmail;

internal sealed class ConfirmEmailHandler(
    UserManager<User> userManager,
    IUsersRepository usersRepository,
    IPublishEndpoint publishEndpoint)
    : ICommandHandler<ConfirmEmailCommand>
{
    private const string WelcomingEmailType = "WelcomingEmail";

    public async Task HandleAsync(ConfirmEmailCommand command, CancellationToken cancellationToken = default)
    {
        var user = await usersRepository.GetByIdAsync(command.UserId, cancellationToken)
            ?? throw new UserNotFoundException(command.UserId);

        if (user.EmailConfirmed)
        {
            return;
        }

        string token;

        try
        {
            token = EmailConfirmationLink.DecodeToken(command.Code);
        }
        catch (FormatException)
        {
            throw new InvalidEmailConfirmationCodeException();
        }

        var result = await userManager.ConfirmEmailAsync(user, token);

        if (!result.Succeeded)
        {
            throw new InvalidEmailConfirmationCodeException();
        }

        await publishEndpoint.Publish(new SendEmailRequestedEvent(
            user.Id,
            user.Email!,
            WelcomingEmailType),
            cancellationToken);
    }
}
