using Microsoft.AspNetCore.Identity;
using StarviaBackend.Modules.Accounts.Application.Users.Queries.GetAccount;
using StarviaBackend.Modules.Accounts.Core.Users.Entities;
using StarviaBackend.Modules.Accounts.Core.Users.Exceptions;
using StarviaBackend.Modules.Accounts.Core.Users.Repositories;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Accounts.Infrastructure.EF.Users.Queries;

internal sealed class GetAccountHandler(IUsersRepository usersRepository, UserManager<User> userManager)
    : IQueryHandler<GetAccountQuery, AccountDto>
{
    public async Task<AccountDto> HandleAsync(GetAccountQuery query, CancellationToken cancellationToken = default)
    {
        var user = await usersRepository.GetByIdAsync(query.UserId, cancellationToken)
                   ?? throw new UserNotFoundException(query.UserId);

        var roles = await userManager.GetRolesAsync(user);
        return new AccountDto(user.Id, user.Email!, user.UserName!, roles.ToArray(), user.CreatedAt);
    }
}
