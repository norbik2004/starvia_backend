using System.Text.Json.Serialization;

namespace StarviaBackend.Modules.Media.Integrations.External;

/// <summary>
/// Wire format of <c>GET v1/media-files/{fileId}</c>. Stays internal so it cannot leak into a module.
/// </summary>
internal sealed record MediaFileApiResponse
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("fileName")]
    public string FileName { get; init; } = string.Empty;

    [JsonPropertyName("url")]
    public string Url { get; init; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; init; }

    [JsonPropertyName("createdBy")]
    public string? CreatedBy { get; init; }
}
