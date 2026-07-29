using StarviaBackend.Modules.Platforms.Core.Platforms.Entities;
using StarviaBackend.Modules.Platforms.Core.Platforms.Enums;
using StarviaBackend.Modules.Platforms.Infrastructure.EF.Platforms.Configurations.Read.Models;

namespace StarviaBackend.Modules.Platforms.Infrastructure.EF.UserPlatforms.Configurations.Read.Models;

internal sealed class UserPlatformReadModel
{
    public Guid Id { get; init; }

    public Guid PlatformId { get; init; }

    public PlatformReadModel Platform { get; init; } = null!;

    public string AccountUsername { get; init; } = null!;

    public string? AccountComment { get; init; }

    public string? ProfilePictureLink { get; init; }

    public string? ExternalAccountId { get; init; }

    public DateTime CreatedAt { get; init; }
}
