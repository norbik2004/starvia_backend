using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using StarviaBackend.Modules.Accounts.Core.Users.Entities;
using StarviaBackend.Modules.Accounts.Core.Users.Exceptions;
using StarviaBackend.Modules.Accounts.Core.Users.Repositories;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Accounts.Application.Users.Commands.GenerateEmailConfirmationToken;

internal sealed class GenerateEmailConfirmationTokenHandler(
   UserManager<User> userManager,
   IUsersRepository usersRepository)
   : ICommandHandler<GenerateEmailConfirmationTokenCommand, GenerateEmailConfirmationTokenResult>
{
    public async Task<GenerateEmailConfirmationTokenResult> HandleAsync(GenerateEmailConfirmationTokenCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await usersRepository.GetByIdAsync(command.UserId, cancellationToken)
            ?? throw new UserNotFoundException(command.UserId);

        if (user.EmailConfirmed)
        {
            throw new EmailArleadyConfirmedException(user.Email!);
        }

        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);

        return new GenerateEmailConfirmationTokenResult(code);
    }
}
