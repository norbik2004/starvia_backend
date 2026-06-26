using AutoMapper;
using AutoMapper.QueryableExtensions;
using Core.Application.DTO.UserUploadedFile.Request;
using Core.Application.DTO.UserUploadedFile.Response;
using Core.Application.Helpers;
using Core.Application.Services;
using Core.Domain.Entities;
using Core.Infrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Service.Exceptions;
using System.Linq.Dynamic.Core;
using System.Runtime.CompilerServices;


namespace Service.Services
{
    public class UserUploadedFileService
        (IUserUploadedFileRepository userUploadedFileRepository, IUserRepository userRepository,
        IMapper mapper, IMinioService minioService)
        : BaseHelpers, IUserUploadedFileService
    {
        public static readonly string UserFilesBucketName = "userfiles";

        public static readonly HashSet<string> SupportedImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".jfif",
            ".png",
            ".gif",
            ".bmp",
            ".webp",
            ".tiff",
            ".tif",
            ".ico",
            ".heic",
            ".heif",
            ".avif"
        };


        public async Task<List<UserUploadedFileResponse>> GetAllUploadedUsersFiles(UploadedFilePaginatedParamsRequest request, string userId)
        {
            var files = userUploadedFileRepository.GetAllAsQueryPerUserIdAsync(userId);

            ValidateQueryParamsDates(request.CreatedBefore, request.CreatedAfter);

            // files = ApplyFilters(request, files);

            var sortColumn = request.SortBy?.ToString() ?? "Id";
            var direction = request.IsAscending ? "asc" : "desc";

            files = files.OrderBy($"{sortColumn} {direction}");

            var entities = await files.ToListAsync();

            return mapper.Map<List<UserUploadedFileResponse>>(entities);
        }

        public async Task<List<UserUploadedFileResponse>> UploadFiles(UserUploadedFileRequest request, string userId)
        {
            if (request.Files.Count == 0)
                throw new BadRequestException("No files provided");

            List<(Guid fileId, string objectPath, IFormFile file)> filesToUpload = [];

            var uploadedFiles = new List<UserUploadedFileResponse>();

            var user = await userRepository.GetByIdAsync(userId);

            if (user == null)
                throw new BadRequestException("Can't send files");

            foreach (var file in request.Files)
            {
                var fileId = Guid.NewGuid();

                var objectPath =
                    $"{user.NormalizedEmail}/{DateTime.UtcNow:yyyy/MM}/{fileId}";

                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();

                if (!SupportedImageExtensions.Contains(fileExtension))
                    throw new BadRequestException($"File type '{fileExtension}' is not supported");

                filesToUpload.Add((fileId, objectPath, file));
            }

            var response = await minioService.SaveFilesAsync(
                [.. filesToUpload.Select(x => (x.objectPath, x.file))],
                UserFilesBucketName
            );

            if (!response)
                throw new BadRequestException("Something went wrong while uploading files");

            foreach (var (fileId, objectPath, file) in filesToUpload)
            {
                var uploadedFile = new UserUploadedFile
                {
                    Id = fileId,
                    UserId = userId,
                    FileName = file.FileName,
                    FilePath = objectPath,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                };

                await userUploadedFileRepository.AddAsync(uploadedFile);

                var mappedFile = mapper.Map<UserUploadedFileResponse>(uploadedFile);
                uploadedFiles.Add(mappedFile);
            }

            await userUploadedFileRepository.SaveChangesAsync();

            return uploadedFiles;
        }
        
        public async Task<FileStreamResult> DownloadFileById(Guid fileId, string userId)
        {
            var file = await userUploadedFileRepository.GetByIdAndUserIdAsync(fileId, userId)
                ?? throw new NotFoundException("Can't download the file");

            var stream = await minioService.DownloadFileAsync(UserFilesBucketName, file.FilePath);

            return new FileStreamResult(stream, "application/octet-stream")
            {
                FileDownloadName = file.FileName
            };
        }

        public async Task<UserUploadedFileResponse> UpdateFileName(UserUploadedFileUpdateRequest request, string userId)
        {
            var file = await userUploadedFileRepository.GetByIdAndUserIdAsync(request.Id, userId)
                ?? throw new NotFoundException("File was not found");

            file.FileName = request.FileName;
            var fileEntity = mapper.Map<UserUploadedFile>(file);

            userUploadedFileRepository.Update(fileEntity);
            await userUploadedFileRepository.SaveChangesAsync();

            return mapper.Map<UserUploadedFileResponse>(file);
        }

        public Task RemoveUploadedFile(Guid fileId, string userId)
        {
            throw new NotImplementedException();
        }
    }
}
