using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Accounts.Application.Users.Queries.GetAccount;

public sealed record GetAccountQuery(Guid UserId) : IQuery<AccountDto>;

public sealed record AccountDto(
    Guid Id,
    string Email,
    string UserName,
    IReadOnlyList<string> Roles,
    DateTime CreatedAt);
