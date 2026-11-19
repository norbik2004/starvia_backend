using StarviaBackend.Shared.Abstractions.Platforms;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Platforms.Application.UserPlatforms.Queries.GetUserPlatformConnection;

/// <summary>
/// Returns OAuth credentials for a connected account. Used by other modules via the platforms integration.
/// Not meant for general UI — AccessToken is sensitive.
/// </summary>
internal sealed record GetUserPlatformConnectionQuery(Guid UserPlatformId) : IQuery<UserPlatformConnectionDto>;

public sealed record UserPlatformConnectionDto(
    Guid Id,
    PlatformType PlatformType,
    string? AccessToken,
    string? ExternalAccountId,
    Guid OwnerId);
