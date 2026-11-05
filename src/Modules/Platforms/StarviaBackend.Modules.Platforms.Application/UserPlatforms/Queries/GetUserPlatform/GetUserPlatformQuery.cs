using StarviaBackend.Modules.Platforms.Core.Platforms.Enums;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Platforms.Application.UserPlatforms.Queries.GetUserPlatform;

/// <summary>One connected account by id, if the caller may view it (see the platforms access policy).</summary>
internal sealed record GetUserPlatformQuery(Guid UserPlatformId) : IQuery<UserPlatformDtoLong>;

// TODO: later on add statistics
public sealed record UserPlatformDtoLong(Guid Id, PlatformType PlatformType, string AccountUsername,
    DateTime CreatedAt, string? ExternalAccountId, string? AccountComment, string? ProfilePictureLink, Guid OwnerId);
