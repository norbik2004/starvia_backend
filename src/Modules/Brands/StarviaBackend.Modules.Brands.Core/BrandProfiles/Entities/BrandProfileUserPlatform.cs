using StarviaBackend.Shared.Abstractions.Domain;

namespace StarviaBackend.Modules.Brands.Core.BrandProfiles.Entities;

internal sealed class BrandProfileUserPlatform : BaseEntity
{
    public Guid BrandProfileId { get; private set; }
    public BrandProfile BrandProfile { get; private set; } = null!;
    public Guid UserPlatformId { get; private set; }
    public bool IsDefault { get; private set; }

    private BrandProfileUserPlatform()
    {
    }

    public static BrandProfileUserPlatform Create(Guid brandProfileId, Guid userPlatformId, bool isDefault) =>
        new()
        {
            Id = Guid.NewGuid(),
            BrandProfileId = brandProfileId,
            UserPlatformId = userPlatformId,
            IsDefault = isDefault,
        };

    internal void SetDefault(bool isDefault) => IsDefault = isDefault;
}
