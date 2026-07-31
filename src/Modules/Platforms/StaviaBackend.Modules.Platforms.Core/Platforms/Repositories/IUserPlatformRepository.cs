using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Modules.Platforms.Core.Platforms.Entities;

namespace StarviaBackend.Modules.Platforms.Core.Platforms.Repositories;

internal interface IUserPlatformRepository
{
    Task<UserPlatform?> GetByIdAndUserIdAsync(Guid userPlatformId,
        Guid UserId, CancellationToken cancellationToken = default);

    Task AddAsync(UserPlatform userPlatform);
}
