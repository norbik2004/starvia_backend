using MassTransit;
using Microsoft.AspNetCore.Identity;
using StarviaBackend.Modules.Accounts.Core.Users.Entities;
using StarviaBackend.Modules.Accounts.Core.Users.Exceptions;
using StarviaBackend.Modules.Accounts.Core.Users.Repositories;
using StarviaBackend.Shared.Abstractions.Commands;
using StarviaBackend.Shared.Abstractions.Contexts;
using StarviaBackend.Shared.Abstractions.Time;

namespace StarviaBackend.Modules.Accounts.Application.Users.Commands.ChangeUserName;

internal sealed class ChangeUserNameHandler(
    IUsersRepository usersRepository)
    : ICommandHandler<ChangeUserNameCommand, ChangeUserNameResult>
{
    public async Task<ChangeUserNameResult> HandleAsync(
        ChangeUserNameCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await usersRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (user == null)
        {
            throw new UserNotFoundException(command.UserId);
        }

        user.ChangeUserName(command.UserName);

        await usersRepository.UpdateAsync(user, cancellationToken);

        return new ChangeUserNameResult(user.Id);
    }
}
