using StarviaBackend.Modules.Media.Core.Media.Exceptions;
using StarviaBackend.Modules.Media.Core.Media.Storage;

namespace StarviaBackend.Modules.Media.Tests.Unit.Fakes;

internal sealed class InMemoryFileStorageService : IFileStorageService
{
    public Dictionary<(string Bucket, string Key), byte[]> Objects { get; } = [];
    public List<(string Bucket, string Key, string ContentType)> Uploads { get; } = [];
    public List<(string Bucket, string Key)> Deletes { get; } = [];

    /// <summary>When set, <see cref="DeleteAsync"/> throws it (simulates a storage outage).</summary>
    public Exception? DeleteFailure { get; set; }

    public async Task UploadAsync(string key, Stream content, string contentType, string bucketName, CancellationToken cancellationToken = default)
    {
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        Objects[(bucketName, key)] = buffer.ToArray();
        Uploads.Add((bucketName, key, contentType));
    }

    public Task<Stream> DownloadAsync(string key, string bucketName, CancellationToken cancellationToken = default)
    {
        if (!Objects.TryGetValue((bucketName, key), out var bytes))
        {
            throw new MediaFileContentNotFoundException(key);
        }

        return Task.FromResult<Stream>(new MemoryStream(bytes));
    }

    public Task DeleteAsync(string key, string bucketName, CancellationToken cancellationToken = default)
    {
        if (DeleteFailure is not null)
        {
            throw DeleteFailure;
        }

        Objects.Remove((bucketName, key));
        Deletes.Add((bucketName, key));
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, string bucketName, CancellationToken cancellationToken = default)
        => Task.FromResult(Objects.ContainsKey((bucketName, key)));

    public string GetPresignedUrl(string key, string bucketName, TimeSpan expiresIn)
        => $"https://storage.test/{bucketName}/{key}";
}
