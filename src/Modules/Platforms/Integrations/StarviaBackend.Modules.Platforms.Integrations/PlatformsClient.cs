using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using StarviaBackend.Modules.Platforms.Integrations.External;
using StarviaBackend.Shared.Infrastructure.Integrations;
using StarviaBackend.Shared.Infrastructure.Serialization;

namespace StarviaBackend.Modules.Platforms.Integrations;

internal sealed class PlatformsClient(HttpClient httpClient) : IPlatformsClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new PlatformTypeJsonConverter() },
    };

    public async Task<UserPlatformConnection> GetConnectionAsync(
        Guid userId,
        Guid userPlatformId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            throw new PlatformsIntegrationException("User id cannot be empty.");
        }

        if (userPlatformId == Guid.Empty)
        {
            throw new PlatformsIntegrationException("User platform id cannot be empty.");
        }

        using var response = await SendAsync($"v1/user-platforms/{userPlatformId}/connection", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw await response.ToFailureAsync(FailureRules(userPlatformId), cancellationToken);
        }

        var payload = await response.Content.ReadFromJsonAsync<UserPlatformConnectionApiResponse>(JsonOptions, cancellationToken)
                      ?? throw new PlatformsIntegrationException("Platforms API returned an empty body.");

        if (payload.OwnerId != userId)
        {
            throw new PlatformsIntegrationNotOwnerException("User is not the owner of the connected platform account.");
        }

        return new UserPlatformConnection(
            payload.Id,
            payload.PlatformType,
            payload.AccessToken,
            payload.ExternalAccountId,
            payload.OwnerId);
    }

    private static IntegrationFailureRules FailureRules(Guid userPlatformId) =>
        new IntegrationFailureRules(status => new PlatformsIntegrationException($"Platforms API returned {(int)status}."))
            .OnStatus(HttpStatusCode.NotFound, () => NotFound(userPlatformId))
            .OnCode("user_platform_not_found", () => NotFound(userPlatformId))
            .OnCode("user_platform_access_denied", () => new PlatformsIntegrationNotOwnerException("User is not the owner of the connected platform account."))
            .OnCode("user_platform_not_owned", () => new PlatformsIntegrationNotOwnerException("User is not the owner of the connected platform account."));

    private static PlatformsIntegrationException NotFound(Guid userPlatformId) =>
        new($"User platform '{userPlatformId}' was not found.");

    private async Task<HttpResponseMessage> SendAsync(string path, CancellationToken cancellationToken)
    {
        try
        {
            return await httpClient.GetAsync(path, cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException
                                   || (ex is TaskCanceledException && !cancellationToken.IsCancellationRequested))
        {
            throw new PlatformsIntegrationException($"Platforms API unreachable: {ex.Message}");
        }
    }
}
