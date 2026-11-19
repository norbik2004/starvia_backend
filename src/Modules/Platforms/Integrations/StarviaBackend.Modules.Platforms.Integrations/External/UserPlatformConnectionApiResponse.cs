using StarviaBackend.Shared.Abstractions.Platforms;

namespace StarviaBackend.Modules.Platforms.Integrations.External;

internal sealed class UserPlatformConnectionApiResponse
{
    public Guid Id { get; init; }
    public PlatformType PlatformType { get; init; }
    public string? AccessToken { get; init; }
    public string? ExternalAccountId { get; init; }
    public Guid OwnerId { get; init; }
}
