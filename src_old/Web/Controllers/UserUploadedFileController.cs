using AutoMapper;
using Core.Application.DTO.Post.Response;
using Core.Application.DTO.UserUploadedFile.Request;
using Core.Application.DTO.UserUploadedFile.Response;
using Core.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Mapping;
using Service.Services;
using System.Reflection.Metadata.Ecma335;
using Web.Helpers;

namespace Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UserUploadedFileController(IUserUploadedFileService userUploadedFileService, IMapper mapper) : ControllerBase
    {
        [HttpGet]
        public async Task<PaginatedList<UserUploadedFileResponse>> GetAllUploadedUsersFiles( [FromQuery] UploadedFilePaginatedParamsRequest request)
        {
            var userId = UserHelpers.GetUserIdFromClaims(User);

            var posts = await userUploadedFileService.GetAllUploadedUsersFiles(request, userId);

            return await PaginatedList<UserUploadedFileResponse>.CreateAsync(posts.AsQueryable(), mapper, request.PageNumber, request.PageSize);
        }

        [HttpGet("download/{fileId:Guid}")]
        public async Task<IActionResult> DownloadFile(Guid fileId)
        {
            var userId = UserHelpers.GetUserIdFromClaims(User);

            var file = await userUploadedFileService.DownloadFileById(fileId, userId);

            return file;
        }

        [HttpPost]
        public async Task<List<UserUploadedFileResponse>> UploadFiles(UserUploadedFileRequest request)
        {
            var userId = UserHelpers.GetUserIdFromClaims(User);

            return await userUploadedFileService.UploadFiles(request, userId);
        }

        [HttpPut]
        public async Task<UserUploadedFileResponse> UpdateFile(UserUploadedFileUpdateRequest request)
        {
            var userId = UserHelpers.GetUserIdFromClaims(User);

            return await userUploadedFileService.UpdateFileName(request, userId);
        }

        [HttpDelete("{fileId:Guid}")]
        public async Task<IActionResult> DeleteFile(Guid fileId)
        {
            var userId = UserHelpers.GetUserIdFromClaims(User);

            await userUploadedFileService.RemoveUploadedFile(fileId, userId);

            return NoContent();
        }
    }
}
