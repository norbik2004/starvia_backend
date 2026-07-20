using Core.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Minio;
using Minio.DataModel.Args;
using Service.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public class MinioService(IMinioClient minioClient, ILogger<MinioService> logger) : IMinioService
    {
        public const int MaxFileSize = 10 * 1024 * 1024; // 10 MB (10 megabajtów)

        public async Task<MemoryStream> DownloadFileAsync(string bucketName, string filePath)
        {
            try
            {
                var memoryStream = new MemoryStream();

                await minioClient.GetObjectAsync(new GetObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(filePath)
                    .WithCallbackStream(stream => stream.CopyTo(memoryStream)));

                memoryStream.Position = 0;

                logger.LogInformation("Successfully downloaded file '{FileName}' from bucket '{BucketName}'", filePath,
                    bucketName);

                return memoryStream;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while downloading file '{FileName}' from bucket '{BucketName}'",
                    filePath, bucketName);
                throw new BadRequestException("Something went wrong while downloading file");
            }
        }

        public async Task<bool> DeleteFileAsync(string bucketName, string filePath)
        {
            try
            {
                await minioClient.RemoveObjectAsync(new RemoveObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(filePath));

                logger.LogInformation("Successfully deleted file '{FileName}' from bucket '{BucketName}'", filePath,
                    bucketName);

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while deleting file '{FileName}' from bucket '{BucketName}'", filePath,
                    bucketName);
                return false;
            }
        }

        public async Task<bool> SaveFilesAsync(List<(string ObjectPath, IFormFile File)> files, string bucketName)
        {
            try
            {
                var bucketExistsArgs = new BucketExistsArgs().WithBucket(bucketName);
                if (!await minioClient.BucketExistsAsync(bucketExistsArgs))
                {
                    await minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName));
                    logger.LogInformation("Bucket '{BucketName}' created", bucketName);
                }

                foreach (var (objectPath, file) in files)
                {
                    if (file.Length > MaxFileSize)
                        throw new BadRequestException("File size is too big");

                    using var stream = file.OpenReadStream();
                    var putArgs = new PutObjectArgs()
                        .WithBucket(bucketName)
                        .WithObject(objectPath)
                        .WithStreamData(stream)
                        .WithObjectSize(file.Length)
                        .WithContentType(file.ContentType);

                    await minioClient.PutObjectAsync(putArgs);

                    logger.LogInformation("Uploaded file '{FileName}' to path '{ObjectPath}' in bucket '{BucketName}'",
                        file.FileName, objectPath, bucketName);
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while saving files to bucket '{BucketName}'", bucketName);
                return false;
            }
        }

    }
}
