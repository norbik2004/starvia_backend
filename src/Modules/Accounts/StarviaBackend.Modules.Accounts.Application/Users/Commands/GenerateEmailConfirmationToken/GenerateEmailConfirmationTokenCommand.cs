using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Accounts.Application.Users.Commands.GenerateEmailConfirmationToken;

public sealed record GenerateEmailConfirmationTokenCommand(Guid UserId)
    : ICommand<GenerateEmailConfirmationTokenResult>;

public sealed record GenerateEmailConfirmationTokenResult(string Token);
