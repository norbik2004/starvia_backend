using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Platforms.Application.LinkedIn.Commands.ConnectLinkedInAccount;

public sealed record ConnectLinkedInAccountCommand(string Code, string State)
    : ICommand<ConnectLinkedInAccountResult>;

public sealed record ConnectLinkedInAccountResult(Guid UserPlatformId, string SuccessRedirectUrl);
