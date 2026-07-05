using AutoMapper;
using Core.Application.DTO.ImageGeneration.File;
using Core.Application.DTO.UserUploadedFile.Response;
using Core.Domain.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Mapping
{
    public class ConversationImageFilePReviewUrlResolver(IHttpContextAccessor httpContextAccessor)
        : IValueResolver<ImagePromptFile, ImagePromptFileResponse, string>
    {

        public string Resolve(
            ImagePromptFile source,
            ImagePromptFileResponse destination,
            string destMember,
            ResolutionContext context)
        {
            var request = httpContextAccessor.HttpContext!.Request;

            return $"{request.Scheme}://{request.Host}/api/ImageConver/download/{source.Id}";  
        }
    }
}
