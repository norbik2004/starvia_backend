using Microsoft.AspNetCore.Identity;
using StarviaBackend.Modules.Accounts.Core.Users.Entities;
using StarviaBackend.Modules.Accounts.Core.Users.Exceptions;
using StarviaBackend.Modules.Accounts.Core.Users.Repositories;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Accounts.Application.Users.Commands.ResetPassword;

internal sealed class ResetPasswordHandler(
    UserManager<User> userManager,
    IUsersRepository usersRepository)
    : ICommandHandler<ResetPasswordCommand>
{
    public async Task HandleAsync(ResetPasswordCommand command, CancellationToken cancellationToken = default)
    {
        var user = await usersRepository.GetByIdAsync(command.UserId, cancellationToken)
            ?? throw new UserNotFoundException(command.UserId);

        string token;
        try
        {
            token = IdentityTokenEncoder.Decode(command.Code);
        }
        catch (FormatException)
        {
            throw new InvalidPasswordResetCodeException();
        }

        var result = await userManager.ResetPasswordAsync(user, token, command.Password);
        if (result.Succeeded)
        {
            return;
        }

        if (result.Errors.Any(e => e.Code.Contains("InvalidToken", StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidPasswordResetCodeException();
        }

        throw new PasswordResetFailedException(string.Join("; ", result.Errors.Select(e => e.Description)));
    }
}
