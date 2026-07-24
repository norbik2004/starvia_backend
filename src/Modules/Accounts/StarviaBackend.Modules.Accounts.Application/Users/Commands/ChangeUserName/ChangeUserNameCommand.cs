using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Accounts.Application.Users.Commands.ChangeUserName;

public sealed record ChangeUserNameCommand(string UserName, Guid UserId)
    : ICommand<ChangeUserNameResult>;

public sealed record ChangeUserNameRequest(string UserName);
public sealed record ChangeUserNameResult(Guid UserId);
