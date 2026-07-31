using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Platforms.Application.UserPlatforms.Commands.AddUserPlatform;

public sealed record AddUserPlatformCommand(AddUserPlatformRequest request, Guid UserId)
    : ICommand<AddUserPlatformResult>;

public sealed record AddUserPlatformRequest(Guid PlatformId, string AccountUserName, string? AccountComment);
public sealed record AddUserPlatformResult(Guid UserPlatformId);
