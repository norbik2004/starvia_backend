using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Platforms.Application.Access;
using StarviaBackend.Modules.Platforms.Application.UserPlatforms.Queries.GetUserPlatform;
using StarviaBackend.Modules.Platforms.Core.Platforms.Access;
using StarviaBackend.Modules.Platforms.Core.Platforms.Exceptions;
using StarviaBackend.Modules.Platforms.Infrastructure.EF.Contexts;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Platforms.Infrastructure.EF.UserPlatforms.Queries;

internal sealed class GetUserPlatformHandler(PlatformsReadDbContext dbContext, IPlatformAccessService access)
    : IQueryHandler<GetUserPlatformQuery, UserPlatformDtoLong>
{
    public async Task<UserPlatformDtoLong> HandleAsync(GetUserPlatformQuery query, CancellationToken cancellationToken = default)
    {
        var userPlatform = await dbContext.UserPlatforms
            .AsNoTracking()
            .Where(u => u.Id == query.UserPlatformId)
            .Select(u => new
            {
                u.Id,
                u.Platform.PlatformType,
                u.AccountUsername,
                u.CreatedAt,
                u.ExternalAccountId,
                u.AccountComment,
                u.ProfilePictureLink,
                u.UserId,
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new UserPlatformNotFoundException(query.UserPlatformId);

        access.EnsureAllowed(PlatformAction.ViewUserPlatform, userPlatform.Id, userPlatform.UserId.ToString());

        return new UserPlatformDtoLong(
            userPlatform.Id,
            userPlatform.PlatformType,
            userPlatform.AccountUsername,
            userPlatform.CreatedAt,
            userPlatform.ExternalAccountId,
            userPlatform.AccountComment,
            userPlatform.ProfilePictureLink,
            userPlatform.UserId);
    }
}
