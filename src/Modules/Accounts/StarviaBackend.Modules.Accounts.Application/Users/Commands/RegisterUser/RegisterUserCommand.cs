using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Accounts.Application.Users.Commands.RegisterUser;

public sealed record RegisterUserCommand(string Email, string Password)
    : ICommand<RegisterUserResult>;

public sealed record RegisterUserResult(Guid UserId);
