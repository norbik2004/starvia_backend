using StarviaBackend.Modules.Media.Integrations;

namespace StarviaBackend.Modules.Posts.Tests.Unit.Fakes;

/// <summary>Stand-in for the media integration: by default every requested file exists.</summary>
internal sealed class FakeMediaClient : IMediaClient
{
    /// <summary>Ids silently left out of the result (to simulate a client that did not return them).</summary>
    public HashSet<Guid> MissingFileIds { get; } = [];

    /// <summary>When set, thrown instead of returning files (e.g. not found / not owner).</summary>
    public Exception? Failure { get; set; }

    public List<(Guid UserId, Guid[] FileIds)> Calls { get; } = [];

    public Task<IReadOnlyList<MediaFile>> GetFilesAsync(
        Guid userId,
        IEnumerable<Guid> fileIds,
        CancellationToken cancellationToken = default)
    {
        var requested = fileIds.ToArray();
        Calls.Add((userId, requested));

        if (Failure is not null)
        {
            throw Failure;
        }

        IReadOnlyList<MediaFile> files = requested
            .Distinct()
            .Where(id => !MissingFileIds.Contains(id))
            .Select(id => new MediaFile(id, $"{id}.png", $"/files/{id}", DateTime.UtcNow))
            .ToList();

        return Task.FromResult(files);
    }
}
