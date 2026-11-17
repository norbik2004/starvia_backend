using StarviaBackend.Modules.Brands.Application.BrandProfiles;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Brands.Application.BrandProfiles.Queries.BrowseBrandProfilesByUserPlatform;

public sealed record BrowseBrandProfilesByUserPlatformQuery(Guid UserPlatformId)
    : IQuery<IReadOnlyList<BrandProfileSummaryDto>>;
