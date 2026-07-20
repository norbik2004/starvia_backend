using Core.Application.DTO.UserUploadedFile.Request;
using Core.Application.DTO.UserUploadedFile.Response;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Services
{
    public interface IUserUploadedFileService
    {
        public Task<List<UserUploadedFileResponse>> GetAllUploadedUsersFiles(UploadedFilePaginatedParamsRequest request,string userId);
        public Task<FileStreamResult> DownloadFileById(Guid fileId, string userId);
        public Task<List<UserUploadedFileResponse>> UploadFiles(UserUploadedFileRequest request, string userId);
        public Task<UserUploadedFileResponse> UpdateFileName(UserUploadedFileUpdateRequest request, string userId);
        public Task RemoveUploadedFile(Guid fileId, string userId);
    }
}
