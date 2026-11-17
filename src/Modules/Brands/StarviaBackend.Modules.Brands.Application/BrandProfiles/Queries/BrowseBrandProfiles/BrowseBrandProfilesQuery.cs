using StarviaBackend.Modules.Brands.Application.BrandProfiles;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Brands.Application.BrandProfiles.Queries.BrowseBrandProfiles;

public sealed record BrowseBrandProfilesQuery(
    int Page = 1,
    int PageSize = 20,
    Guid? UserPlatformId = null,
    Guid? OwnerId = null) : IPagedQuery<BrandProfileSummaryDto>
{
    public int Page { get; init; } = Page < 1 ? 1 : Page;
    public int PageSize { get; init; } = PageSize is < 1 or > 100 ? 20 : PageSize;
}

public enum BrandProfileSortBy
{
    CreatedAt,
    Name,
}
