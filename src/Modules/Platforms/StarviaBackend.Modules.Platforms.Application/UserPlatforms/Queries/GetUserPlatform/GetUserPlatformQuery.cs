using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Modules.Platforms.Application.UserPlatforms.Queries.BrowseUserPlatforms;
using StarviaBackend.Modules.Platforms.Core.Platforms.Enums;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Platforms.Application.UserPlatforms.Queries.GetUserPlatform;

internal sealed record GetUserPlatformQuery(Guid UserId, Guid UserPlatformId) : IQuery<UserPlatformDtoLong>;

// TODO: later on add statistics
public sealed record UserPlatformDtoLong(Guid Id, PlatformType PlatformType, string AccountUsername,
    DateTime CreatedAt, string? ExternalAccountId, string? AccountComment, string? ProfilePictureLink);
