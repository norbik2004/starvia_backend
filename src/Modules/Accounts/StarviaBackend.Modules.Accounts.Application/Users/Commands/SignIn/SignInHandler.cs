using Microsoft.AspNetCore.Identity;
using StarviaBackend.Modules.Accounts.Core.Users.Entities;
using StarviaBackend.Modules.Accounts.Core.Users.Exceptions;
using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Accounts.Application.Users.Commands.SignIn;

internal sealed class SignInHandler(UserManager<User> userManager, IAuthManager authManager)
    : ICommandHandler<SignInCommand, JsonWebToken>
{
    public async Task<JsonWebToken> HandleAsync(SignInCommand command, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(command.Email);
        if (user is null || !await userManager.CheckPasswordAsync(user, command.Password))
        {
            throw new InvalidCredentialsException();
        }

        var roles = await userManager.GetRolesAsync(user);
        return authManager.CreateToken(user.Id.ToString(), user.Email!, roles);
    }
}
