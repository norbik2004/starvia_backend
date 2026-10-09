using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StarviaBackend.Modules.Media.Core.Media.Exceptions;
using StarviaBackend.Modules.Media.Core.Media.Storage;
using StarviaBackend.Shared.Abstractions.Storage;

namespace StarviaBackend.Modules.Media.Infrastructure.Storage.RustFs;

internal sealed class RustFsFileStorageService(
    IAmazonS3 client,
    ILogger<RustFsFileStorageService> logger) : IFileStorageService
{
    private readonly SemaphoreSlim _bucketLock = new(1, 1);
    private volatile bool _bucketReady;

    public async Task UploadAsync(string key, Stream content, string contentType, string bucketName,
        CancellationToken cancellationToken = default)
    {
        await EnsureBucketAsync(bucketName, cancellationToken);

        var buffered = false;
        if (!content.CanSeek)
        {
            var memory = new MemoryStream();
            await content.CopyToAsync(memory, cancellationToken);
            memory.Position = 0;
            content = memory;
            buffered = true;
        }

        try
        {
            var request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = key,
                InputStream = content,
                ContentType = contentType,
                AutoCloseStream = false
            };

            await client.PutObjectAsync(request, cancellationToken);
            logger.LogInformation("Uploaded object {Key} to bucket {Bucket}", key, bucketName);
        }
        finally
        {
            if (buffered)
            {
                await content.DisposeAsync();
            }
        }
    }

    public async Task<Stream> DownloadAsync(string key, string bucketName, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await client.GetObjectAsync(bucketName, key, cancellationToken);
            return response.ResponseStream;
        }
        catch (AmazonS3Exception ex) when (IsNotFound(ex))
        {
            throw new MediaFileContentNotFoundException(key);
        }
    }

    public async Task DeleteAsync(string key, string bucketName, CancellationToken cancellationToken = default)
    {
        // S3 delete is idempotent: deleting a missing key is not an error.
        await client.DeleteObjectAsync(bucketName, key, cancellationToken);
        logger.LogInformation("Deleted object {Key} from bucket {Bucket}", key, bucketName);
    }

    public async Task<bool> ExistsAsync(string key, string bucketName, CancellationToken cancellationToken = default)
    {
        try
        {
            await client.GetObjectMetadataAsync(bucketName, key, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex) when (IsNotFound(ex))
        {
            return false;
        }
    }

    public string GetPresignedUrl(string key, string bucketName, TimeSpan expiresIn)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = bucketName,
            Key = key,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.Add(expiresIn)
        };

        return client.GetPreSignedURL(request);
    }

    /// <summary>Creates the bucket on first use (idempotent, thread-safe).</summary>
    private async Task EnsureBucketAsync(string bucketName, CancellationToken cancellationToken)
    {
        if (_bucketReady)
        {
            return;
        }

        await _bucketLock.WaitAsync(cancellationToken);
        try
        {
            if (_bucketReady)
            {
                return;
            }

            if (!await AmazonS3Util.DoesS3BucketExistV2Async(client, bucketName))
            {
                await client.PutBucketAsync(bucketName, cancellationToken);
                logger.LogInformation("Created bucket {Bucket}", bucketName);
            }

            _bucketReady = true;
        }
        finally
        {
            _bucketLock.Release();
        }
    }

    private static bool IsNotFound(AmazonS3Exception ex) =>
        ex.StatusCode == HttpStatusCode.NotFound || ex.ErrorCode is "NoSuchKey" or "NotFound";
}
