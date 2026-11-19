namespace StarviaBackend.Modules.Media.Integrations;

/// <summary>
/// The port other modules depend on. Callers speak in these terms and never see the media API's DTOs.
/// </summary>
public interface IMediaClient
{
    Task<IReadOnlyList<MediaFile>> GetFilesAsync(
        Guid userId,
        IEnumerable<Guid> fileIds,
        CancellationToken cancellationToken = default);

    Task<MediaFileContent> DownloadContentAsync(Guid fileId, CancellationToken cancellationToken = default);
}

/// <summary>Domain-shaped file returned by the media integration.</summary>
public sealed record MediaFile(Guid Id, string FileName, string Url, DateTime CreatedAt);

/// <summary>Raw file payload returned by <see cref="IMediaClient.DownloadContentAsync"/>.</summary>
public sealed record MediaFileContent(Stream Content, string ContentType, string? FileName);
