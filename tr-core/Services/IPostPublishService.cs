using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tr_core.DTO.PostPublication.Request;
using tr_core.DTO.PostPublication.Response;

namespace tr_core.Services
{
    public interface IPostPublishService
    {
        public Task<PostPublicationResponse> PublishPostToLinkedInAsync(PostPublicationRequest request, string userId);
    }
}
