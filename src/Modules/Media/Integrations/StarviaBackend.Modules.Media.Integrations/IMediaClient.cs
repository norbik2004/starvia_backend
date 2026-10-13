namespace StarviaBackend.Modules.Media.Integrations;

/// <summary>
/// The port other modules depend on. Callers speak in these terms and never see the media API's DTOs.
/// </summary>
public interface IMediaClient
{
    Task<IReadOnlyList<MediaFile>> GetFilesAsync(Guid userId, IEnumerable<Guid> fileIds, CancellationToken cancellationToken = default);
}

/// <summary>Domain-shaped file returned by the media integration.</summary>
public sealed record MediaFile(Guid Id, string FileName, string Url, DateTime CreatedAt);
