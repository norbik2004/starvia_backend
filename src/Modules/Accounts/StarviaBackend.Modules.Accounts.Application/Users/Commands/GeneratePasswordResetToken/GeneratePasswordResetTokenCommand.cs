using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Accounts.Application.Users.Commands.GeneratePasswordResetToken;

public sealed record GeneratePasswordResetTokenCommand(Guid UserId)
    : ICommand<GeneratePasswordResetTokenResult>;

public sealed record GeneratePasswordResetTokenResult(string Token);
