using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Accounts.Application.Users.Commands.ResetPassword;

public sealed record ResetPasswordCommand(Guid UserId, string Code, string Password) : ICommand;
