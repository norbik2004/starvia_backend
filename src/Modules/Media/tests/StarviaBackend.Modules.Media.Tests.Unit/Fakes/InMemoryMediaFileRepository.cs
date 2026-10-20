using StarviaBackend.Modules.Media.Core.Media.Entities;
using StarviaBackend.Modules.Media.Core.Media.Enums;
using StarviaBackend.Modules.Media.Core.Media.Repositories;

namespace StarviaBackend.Modules.Media.Tests.Unit.Fakes;

internal sealed class InMemoryMediaFileRepository : IMediaFileRepository
{
    public List<MediaFile> Files { get; } = [];
    public List<MediaFile> Updated { get; } = [];
    public List<MediaFile> Removed { get; } = [];

    /// <summary>When set, <see cref="AddAsync"/> throws it (simulates a database failure).</summary>
    public Exception? AddFailure { get; set; }

    public Task AddAsync(MediaFile mediaFile, CancellationToken cancellationToken = default)
    {
        if (AddFailure is not null)
        {
            throw AddFailure;
        }

        Files.Add(mediaFile);
        return Task.CompletedTask;
    }

    public Task<MediaFile?> GetByIdAsync(Guid fileId, CancellationToken cancellationToken = default)
        => Task.FromResult(Files.FirstOrDefault(f => f.Id == fileId));

    public Task UpdateAsync(MediaFile mediaFile, CancellationToken cancellationToken = default)
    {
        Updated.Add(mediaFile);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(MediaFile mediaFile, CancellationToken cancellationToken = default)
    {
        Files.Remove(mediaFile);
        Removed.Add(mediaFile);
        return Task.CompletedTask;
    }

    /// <summary>Seeds a file "created" by the given user, as the audit interceptor would.</summary>
    public MediaFile Seed(Guid ownerId, string filePath = "owner/file_1.png", string? description = "Seeded")
    {
        var file = MediaFile.Create(Guid.NewGuid(), "file.png", filePath, FileSource.User, FileType.Image, description);
        file.CreatedBy = ownerId.ToString();
        Files.Add(file);
        return file;
    }
}
