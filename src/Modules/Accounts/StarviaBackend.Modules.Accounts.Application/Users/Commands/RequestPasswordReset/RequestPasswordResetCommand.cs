using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Accounts.Application.Users.Commands.RequestPasswordReset;

public sealed record RequestPasswordResetCommand(string Email) : ICommand;
