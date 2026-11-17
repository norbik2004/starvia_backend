using Microsoft.EntityFrameworkCore;
using StarviaBackend.Modules.Brands.Application.BrandProfiles;
using StarviaBackend.Modules.Brands.Application.BrandProfiles.Access;
using StarviaBackend.Modules.Brands.Application.BrandProfiles.Queries.GetBrandProfile;
using StarviaBackend.Modules.Brands.Core.BrandProfiles.Access;
using StarviaBackend.Modules.Brands.Core.BrandProfiles.Exceptions;
using StarviaBackend.Modules.Brands.Infrastructure.EF.Contexts;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Brands.Infrastructure.EF.BrandProfiles.Queries;

internal sealed class GetBrandProfileHandler(BrandsReadDbContext dbContext, IBrandAccessService access)
    : IQueryHandler<GetBrandProfileQuery, BrandProfileDto>
{
    public async Task<BrandProfileDto> HandleAsync(
        GetBrandProfileQuery query,
        CancellationToken cancellationToken = default)
    {
        var profile = await dbContext.BrandProfiles
            .AsNoTracking()
            .Include(p => p.UserPlatforms)
            .Include(p => p.Platforms)
            .FirstOrDefaultAsync(p => p.Id == query.BrandProfileId, cancellationToken)
            ?? throw new BrandProfileNotFoundException(query.BrandProfileId);

        access.EnsureAllowed(BrandAction.View, profile.Id, profile.UserId.ToString());

        return new BrandProfileDto(
            profile.Id,
            profile.Name,
            profile.Brief,
            profile.GenerationDefaults,
            profile.Platforms
                .Select(p => p.PlatformType)
                .OrderBy(p => p)
                .ToList(),
            profile.UserPlatforms
                .Select(l => new BrandProfileUserPlatformDto(l.UserPlatformId, l.IsDefault))
                .ToList(),
            profile.CreatedAt,
            profile.UserId);
    }
}
