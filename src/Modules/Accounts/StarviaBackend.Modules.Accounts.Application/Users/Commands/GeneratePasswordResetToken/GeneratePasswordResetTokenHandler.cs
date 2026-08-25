using Microsoft.AspNetCore.Identity;
using StarviaBackend.Modules.Accounts.Core.Users.Entities;
using StarviaBackend.Modules.Accounts.Core.Users.Exceptions;
using StarviaBackend.Modules.Accounts.Core.Users.Repositories;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Accounts.Application.Users.Commands.GeneratePasswordResetToken;

internal sealed class GeneratePasswordResetTokenHandler(
    UserManager<User> userManager,
    IUsersRepository usersRepository)
    : ICommandHandler<GeneratePasswordResetTokenCommand, GeneratePasswordResetTokenResult>
{
    public async Task<GeneratePasswordResetTokenResult> HandleAsync(
        GeneratePasswordResetTokenCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await usersRepository.GetByIdAsync(command.UserId, cancellationToken)
            ?? throw new UserNotFoundException(command.UserId);

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        return new GeneratePasswordResetTokenResult(token);
    }
}
