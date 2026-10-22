using StarviaBackend.Modules.Media.Integrations;

namespace StarviaBackend.Modules.Posts.Tests.Integration;

/// <summary>
/// Replaces the HTTP media integration so the Posts tests do not need the media module or storage.
/// By default every requested file exists and belongs to the caller.
/// </summary>
public sealed class FakeMediaClient : IMediaClient
{
    /// <summary>Files that "do not exist" in media; requesting them fails like the real client does.</summary>
    public HashSet<Guid> UnknownFileIds { get; } = [];

    /// <summary>Files owned by somebody else; requesting them fails like the real client does.</summary>
    public HashSet<Guid> ForeignFileIds { get; } = [];

    public void Reset()
    {
        UnknownFileIds.Clear();
        ForeignFileIds.Clear();
    }

    public Task<IReadOnlyList<MediaFile>> GetFilesAsync(
        Guid userId,
        IEnumerable<Guid> fileIds,
        CancellationToken cancellationToken = default)
    {
        var files = new List<MediaFile>();

        foreach (var fileId in fileIds.Distinct())
        {
            if (UnknownFileIds.Contains(fileId))
            {
                throw new MediaIntegrationException($"File '{fileId}' was not found in the media API.");
            }

            if (ForeignFileIds.Contains(fileId))
            {
                throw new MediaIntegrationNotOwnerException("User is not the owner of file.");
            }

            files.Add(new MediaFile(fileId, $"{fileId}.png", $"/files/{fileId}", DateTime.UtcNow));
        }

        return Task.FromResult<IReadOnlyList<MediaFile>>(files);
    }
}
