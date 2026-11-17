using StarviaBackend.Modules.Brands.Core.BrandProfiles.Models;
using StarviaBackend.Shared.Abstractions.Platforms;

namespace StarviaBackend.Modules.Brands.Application.BrandProfiles;

public sealed record BrandProfileDto(
    Guid Id,
    string Name,
    BrandBrief Brief,
    BrandGenerationDefaults? GenerationDefaults,
    IReadOnlyList<PlatformType> Platforms,
    IReadOnlyList<BrandProfileUserPlatformDto> UserPlatforms,
    DateTime CreatedAt,
    Guid OwnerId);

public sealed record BrandProfileSummaryDto(
    Guid Id,
    string Name,
    DateTime CreatedAt,
    Guid OwnerId,
    int LinkedUserPlatformCount);

public sealed record BrandProfileUserPlatformDto(Guid UserPlatformId, bool IsDefault);

public sealed record BrandProfileUserPlatformLink(Guid UserPlatformId, bool IsDefault = false);
