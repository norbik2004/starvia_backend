using System.Text.Json.Serialization;
using StarviaBackend.Shared.Abstractions.Platforms;

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

    [JsonPropertyName("body")]
    public string Body { get; init; } = string.Empty;

    [JsonPropertyName("platforms")]
    public IReadOnlyList<PlatformType> Platforms { get; init; } = [];

    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; init; }

    [JsonPropertyName("createdBy")]
    public string? CreatedBy { get; init; }

    [JsonPropertyName("postAttachments")]
    public IReadOnlyList<PostAttachmentApiResponse> PostAttachments { get; init; } = [];
}

internal sealed record PostAttachmentApiResponse
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("fileId")]
    public Guid FileId { get; init; }

    [JsonPropertyName("order")]
    public int Order { get; init; }
}
