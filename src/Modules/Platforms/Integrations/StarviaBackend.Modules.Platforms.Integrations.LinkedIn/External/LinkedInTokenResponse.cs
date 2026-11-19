using System.Text.Json.Serialization;

namespace StarviaBackend.Modules.Platforms.Integrations.LinkedIn.External;

internal sealed class LinkedInTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; } = string.Empty;
}

internal sealed class LinkedInUserInfoResponse
{
    [JsonPropertyName("sub")]
    public string Sub { get; init; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("picture")]
    public string? Picture { get; init; }
}

internal sealed class LinkedInUgcPostResponse
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;
}
