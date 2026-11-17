using StarviaBackend.Shared.Abstractions.Domain;
using StarviaBackend.Shared.Abstractions.Platforms;

namespace StarviaBackend.Modules.Brands.Core.BrandProfiles.Entities;

internal sealed class BrandProfilePlatform : BaseEntity
{
    public Guid BrandProfileId { get; private set; }
    public BrandProfile BrandProfile { get; private set; } = null!;
    public PlatformType PlatformType { get; private set; }

    private BrandProfilePlatform()
    {
    }

    public static BrandProfilePlatform Create(Guid brandProfileId, PlatformType platformType) =>
        new()
        {
            Id = Guid.NewGuid(),
            BrandProfileId = brandProfileId,
            PlatformType = platformType,
        };
}
