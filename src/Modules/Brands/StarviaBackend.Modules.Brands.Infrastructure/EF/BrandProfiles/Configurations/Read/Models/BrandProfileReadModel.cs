using StarviaBackend.Modules.Brands.Core.BrandProfiles.Models;
using StarviaBackend.Shared.Abstractions.Platforms;

namespace StarviaBackend.Modules.Brands.Infrastructure.EF.BrandProfiles.Configurations.Read.Models;

internal sealed class BrandProfileReadModel
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public string Name { get; init; } = null!;
    public BrandBrief Brief { get; init; } = null!;
    public BrandGenerationDefaults? GenerationDefaults { get; init; }
    public DateTime CreatedAt { get; init; }
    public ICollection<BrandProfileUserPlatformReadModel> UserPlatforms { get; init; } = [];
    public ICollection<BrandProfilePlatformReadModel> Platforms { get; init; } = [];
}

internal sealed class BrandProfileUserPlatformReadModel
{
    public Guid Id { get; init; }
    public Guid BrandProfileId { get; init; }
    public Guid UserPlatformId { get; init; }
    public bool IsDefault { get; init; }
    public BrandProfileReadModel BrandProfile { get; init; } = null!;
}

internal sealed class BrandProfilePlatformReadModel
{
    public Guid Id { get; init; }
    public Guid BrandProfileId { get; init; }
    public PlatformType PlatformType { get; init; }
    public BrandProfileReadModel BrandProfile { get; init; } = null!;
}
