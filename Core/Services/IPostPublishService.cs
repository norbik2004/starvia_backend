using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DTO.PostPublication.Request;
using Core.DTO.PostPublication.Response;

namespace Core.Services
{
    public interface IPostPublishService
    {
        public Task<PostPublicationResponse> PublishPostToLinkedInAsync(PublishPostRequest request, string userId);
    }
}
