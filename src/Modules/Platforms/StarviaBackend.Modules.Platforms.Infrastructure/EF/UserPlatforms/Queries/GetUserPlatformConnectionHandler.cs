using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Platforms.Application.Access;
using StarviaBackend.Modules.Platforms.Application.UserPlatforms.Queries.GetUserPlatformConnection;
using StarviaBackend.Modules.Platforms.Core.Platforms.Access;
using StarviaBackend.Modules.Platforms.Core.Platforms.Exceptions;
using StarviaBackend.Modules.Platforms.Infrastructure.EF.Contexts;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Platforms.Infrastructure.EF.UserPlatforms.Queries;

internal sealed class GetUserPlatformConnectionHandler(
    PlatformsWriteDbContext dbContext,
    IPlatformAccessService access)
    : IQueryHandler<GetUserPlatformConnectionQuery, UserPlatformConnectionDto>
{
    public async Task<UserPlatformConnectionDto> HandleAsync(
        GetUserPlatformConnectionQuery query,
        CancellationToken cancellationToken = default)
    {
        var userPlatform = await dbContext.UserPlatforms
            .AsNoTracking()
            .Where(u => u.Id == query.UserPlatformId)
            .Select(u => new
            {
                u.Id,
                u.Platform.PlatformType,
                u.AccessToken,
                u.ExternalAccountId,
                u.UserId,
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new UserPlatformNotFoundException(query.UserPlatformId);

        access.EnsureAllowed(PlatformAction.ViewUserPlatform, userPlatform.Id, userPlatform.UserId.ToString());

        return new UserPlatformConnectionDto(
            userPlatform.Id,
            userPlatform.PlatformType,
            userPlatform.AccessToken,
            userPlatform.ExternalAccountId,
            userPlatform.UserId);
    }
}
