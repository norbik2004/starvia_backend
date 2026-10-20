using System.Collections.Concurrent;
using StarviaBackend.Modules.Media.Core.Media.Exceptions;
using StarviaBackend.Modules.Media.Core.Media.Storage;

namespace StarviaBackend.Modules.Media.Tests.Integration;

/// <summary>Replaces the RustFS/S3 storage so the Media tests need neither the bucket nor the network.</summary>
internal sealed class InMemoryFileStorageService : IFileStorageService
{
    private readonly ConcurrentDictionary<(string Bucket, string Key), byte[]> _objects = new();

    /// <summary>When true, <see cref="DeleteAsync"/> fails like a storage outage would.</summary>
    public bool FailDeletes { get; set; }

    public IReadOnlyCollection<(string Bucket, string Key)> Keys => _objects.Keys.ToList();

    public bool Contains(string key, string bucketName) => _objects.ContainsKey((bucketName, key));

    public byte[]? Read(string key, string bucketName) =>
        _objects.TryGetValue((bucketName, key), out var bytes) ? bytes : null;

    public void Reset()
    {
        FailDeletes = false;
        _objects.Clear();
    }

    public async Task UploadAsync(string key, Stream content, string contentType, string bucketName, CancellationToken cancellationToken = default)
    {
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, cancellationToken);
        _objects[(bucketName, key)] = buffer.ToArray();
    }

    public Task<Stream> DownloadAsync(string key, string bucketName, CancellationToken cancellationToken = default)
    {
        if (!_objects.TryGetValue((bucketName, key), out var bytes))
        {
            throw new MediaFileContentNotFoundException(key);
        }

        return Task.FromResult<Stream>(new MemoryStream(bytes));
    }

    public Task DeleteAsync(string key, string bucketName, CancellationToken cancellationToken = default)
    {
        if (FailDeletes)
        {
            throw new IOException("Storage is unavailable.");
        }

        _objects.TryRemove((bucketName, key), out _);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, string bucketName, CancellationToken cancellationToken = default)
        => Task.FromResult(_objects.ContainsKey((bucketName, key)));

    public string GetPresignedUrl(string key, string bucketName, TimeSpan expiresIn)
        => $"https://storage.test/{bucketName}/{key}";
}
