using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;
using StarviaBackend.Modules.Platforms.Application.UserPlatforms.Queries.GetUserPlatform;
using StarviaBackend.Modules.Platforms.Core.Platforms.Exceptions;
using StarviaBackend.Modules.Platforms.Core.Platforms.Repositories;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Platforms.Infrastructure.EF.UserPlatforms.Queries;

internal sealed class GetUserPlatformHandler(IUserPlatformRepository userPlatformsRepository)
    : IQueryHandler<GetUserPlatformQuery, UserPlatformDtoLong>
{
    public async Task<UserPlatformDtoLong> HandleAsync(GetUserPlatformQuery query, CancellationToken cancellationToken = default)
    {
        var userPlatform = await userPlatformsRepository.GetByIdAndUserIdAsync(query.UserPlatformId, query.UserId, cancellationToken)
                   ?? throw new UserPlatformNotFoundException(query.UserPlatformId);

        return new UserPlatformDtoLong(userPlatform.Id, userPlatform.Platform.PlatformType, userPlatform.AccountUsername,
            userPlatform.CreatedAt, userPlatform.ExternalAccountId, userPlatform.AccountComment, userPlatform.ProfilePictureLink);
    }
}
