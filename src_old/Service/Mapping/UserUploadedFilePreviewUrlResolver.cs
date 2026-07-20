using AutoMapper;
using Core.Application.DTO.UserUploadedFile.Response;
using Core.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Service.Mapping
{
    public class UserUploadedFilePreviewUrlResolver(IHttpContextAccessor httpContextAccessor)
        : IValueResolver<UserUploadedFile, UserUploadedFileResponse, string>
    {

        public string Resolve(
            UserUploadedFile source,
            UserUploadedFileResponse destination,
            string destMember,
            ResolutionContext context)
        {
            var request = httpContextAccessor.HttpContext!.Request;

            return $"{request.Scheme}://{request.Host}/api/UserUploadedFile/download/{source.Id}";
        }
    }
}
