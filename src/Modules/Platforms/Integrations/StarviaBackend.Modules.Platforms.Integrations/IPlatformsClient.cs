using StarviaBackend.Shared.Abstractions.Platforms;

namespace StarviaBackend.Modules.Platforms.Integrations;

/// <summary>
/// The port other modules depend on. Callers speak in these terms and never see the platforms API's DTOs.
/// </summary>
public interface IPlatformsClient
{
    /// <summary>
    /// Loads a connected account (including OAuth token) on behalf of <paramref name="userId"/>.
    /// Throws when missing or not owned by the user.
    /// </summary>
    Task<UserPlatformConnection> GetConnectionAsync(
        Guid userId,
        Guid userPlatformId,
        CancellationToken cancellationToken = default);
}

/// <summary>Domain-shaped connected account returned by the platforms integration.</summary>
public sealed record UserPlatformConnection(
    Guid Id,
    PlatformType PlatformType,
    string? AccessToken,
    string? ExternalAccountId,
    Guid OwnerId);
