using StarviaBackend.Modules.Brands.Application.BrandProfiles;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Brands.Application.BrandProfiles.Queries.GetBrandProfile;

public sealed record GetBrandProfileQuery(Guid BrandProfileId) : IQuery<BrandProfileDto>;
