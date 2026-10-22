using Microsoft.AspNetCore.Identity;
using StarviaBackend.Modules.Accounts.Core.Users.Entities;
using StarviaBackend.Modules.Accounts.Core.Users.Exceptions;
using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Commands;
using StarviaBackend.Shared.Abstractions.Time;

namespace StarviaBackend.Modules.Accounts.Application.Users.Commands.SignIn;

internal sealed class SignInHandler(UserManager<User> userManager, IAuthManager authManager, IClock clock)
    : ICommandHandler<SignInCommand, JsonWebToken>
{
    public async Task<JsonWebToken> HandleAsync(SignInCommand command, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(command.Email);

        if (user is null || !await userManager.CheckPasswordAsync(user, command.Password))
        {
            throw new InvalidCredentialsException();
        }

        if (!user.EmailConfirmed)
        {
            throw new EmailNotConfirmedException();
        }

        var roles = await userManager.GetRolesAsync(user);

        user.RecordLogin(clock.UtcNow);

        return authManager.CreateToken(user.Id.ToString(), user.Email!, roles);
    }
}
