using System.Text.Json.Serialization;

namespace StarviaBackend.Modules.Posts.Integrations.External;

/// <summary>
/// Wire format of <c>POST v1/post-publications</c>. Stays internal so it cannot leak into a module.
/// </summary>
internal sealed record PostPublicationApiResponse
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("postId")]
    public Guid PostId { get; init; }

    [JsonPropertyName("externalPostId")]
    public string? ExternalPostId { get; init; }

    [JsonPropertyName("publishedAt")]
    public DateTime? PublishedAt { get; init; }
}
