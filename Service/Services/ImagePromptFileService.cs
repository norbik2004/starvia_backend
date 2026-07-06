using Core.Application.Services;
using Core.Domain.Consts;
using Core.Infrastructure.Repositories;
using Microsoft.AspNetCore.Mvc;
using Repository.Repositories;
using Service.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services
{
    public class ImagePromptFileService(IImagePromptFileRepository imagePromptFileRepository,
        IMinioService minioService) : IImagePromptFileService
    {
        public async Task<FileStreamResult> DownloadFileById(Guid fileId, string userId)
        {
            var file = imagePromptFileRepository.GetByIdAndUserId(fileId, userId)
                ?? throw new NotFoundException("Can't download the file");

            var stream = await minioService.DownloadFileAsync(BucketNames.GeminiGeneratedImagesBucketName, file.FilePath);

            return new FileStreamResult(stream, "application/octet-stream")
            {
                FileDownloadName = file.FileName
            };
        }
    }
}
