using System.Text.Json.Serialization;

namespace StarviaBackend.Modules.Posts.Integrations.External;

/// <summary>
/// Wire format of <c>GET v1/posts/{postId}</c>. Stays internal so it cannot leak into a module.
/// </summary>
internal sealed record PostApiResponse
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; init; }

    [JsonPropertyName("createdBy")]
    public string? CreatedBy { get; init; }
}
