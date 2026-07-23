using MassTransit;
using Microsoft.AspNetCore.Identity;
using StarviaBackend.Modules.Accounts.Core.Users.Entities;
using StarviaBackend.Modules.Accounts.Core.Users.Exceptions;
using StarviaBackend.Modules.Accounts.Core.Users.Repositories;
using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Commands;
using StarviaBackend.Shared.Abstractions.Time;
using StarviaBackend.Modules.Accounts.Application.Users.Events.UserRegistered;

namespace StarviaBackend.Modules.Accounts.Application.Users.Commands.RegisterUser;

internal sealed class RegisterUserHandler(
    UserManager<User> userManager,
    IUsersRepository usersRepository,
    IPublishEndpoint publishEndpoint,
    IClock clock)
    : ICommandHandler<RegisterUserCommand, RegisterUserResult>
{
    public async Task<RegisterUserResult> HandleAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken = default)
    {
        if (await usersRepository.ExistsByEmailAsync(command.Email, cancellationToken))
        {
            throw new EmailAlreadyInUseException(command.Email);
        }

        var user = User.Create(command.Email, command.UserName, clock.UtcNow);

        var result = await userManager.CreateAsync(user, command.Password);
        if (!result.Succeeded)
        {
            throw new UserCreationFailedException(
                string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        await userManager.AddToRoleAsync(user, UserRoles.User);

        await publishEndpoint.Publish(
            new UserRegisteredEvent(user.Id, command.Email, clock.UtcNow),
            cancellationToken);

        return new RegisterUserResult(user.Id);
    }
}
