namespace StarviaBackend.Modules.Media.Core.Media.Storage;

/// <summary>Abstraction over the object storage (RustFS) used to keep media file content.</summary>
internal interface IFileStorageService
{
    /// <summary>Uploads <paramref name="content"/> under the given object <paramref name="key"/>.</summary>
    Task UploadAsync(string key, Stream content, string contentType, string bucketName, CancellationToken cancellationToken = default);

    /// <summary>Opens a read stream for the object. Throws when the object does not exist.</summary>
    Task<Stream> DownloadAsync(string key, string bucketName, CancellationToken cancellationToken = default);

    /// <summary>Deletes the object. Does nothing when the object does not exist.</summary>
    Task DeleteAsync(string key, string bucketName, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string key, string bucketName, CancellationToken cancellationToken = default);

    /// <summary>Builds a time-limited, pre-signed GET url for the object.</summary>
    string GetPresignedUrl(string key, string bucketName, TimeSpan expiresIn);
}
