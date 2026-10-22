using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Accounts.Application.Users.Commands.ConfirmEmail;

public sealed record ConfirmEmailCommand(Guid UserId, string Code) : ICommand;
