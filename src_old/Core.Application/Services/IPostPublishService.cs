using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Application.DTO.PostPublication.Request;
using Core.Application.DTO.PostPublication.Response;

namespace Core.Application.Services
{
    public interface IPostPublishService
    {
        public Task<PostPublicationResponse> PublishPostToLinkedInAsync(PublishPostRequest request, string userId);
        public Task<List<PostPublicationResponse>> GetUserPostPublicationsAsync(PostPublicationParamsRequest request, string userId);
    }
}
