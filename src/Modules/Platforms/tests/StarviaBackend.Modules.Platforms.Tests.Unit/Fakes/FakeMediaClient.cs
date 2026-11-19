using StarviaBackend.Modules.Media.Integrations;

namespace StarviaBackend.Modules.Platforms.Tests.Unit.Fakes;

internal sealed class FakeMediaClient : IMediaClient
{
    public byte[] ContentBytes { get; set; } = [1, 2, 3];
    public string ContentType { get; set; } = "image/png";

    public Task<IReadOnlyList<MediaFile>> GetFilesAsync(
        Guid userId,
        IEnumerable<Guid> fileIds,
        CancellationToken cancellationToken = default)
    {
        var files = fileIds
            .Distinct()
            .Select(id => new MediaFile(id, $"{id}.png", $"http://media.test/files/{id}", DateTime.UtcNow))
            .ToList();

        return Task.FromResult<IReadOnlyList<MediaFile>>(files);
    }

    public Task<MediaFileContent> DownloadContentAsync(
        Guid fileId,
        CancellationToken cancellationToken = default)
    {
        var stream = new MemoryStream(ContentBytes);
        return Task.FromResult(new MediaFileContent(stream, ContentType, $"{fileId}.png"));
    }
}
