using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tr_core.DTO.LinkedIn.Request;
using tr_core.DTO.PostPublication.Request;
using tr_core.DTO.PostPublication.Response;
using tr_core.Enums;
using tr_core.Services;
using tr_service.Exceptions;

namespace tr_service.Services
{
    public class PostPublishService(IMapper mapper, IPostService postService, IUserPlatformService userPlatformService,
        ILinkedInService linkedInService) : IPostPublishService
    {
        public async Task<PostPublicationResponse> PublishPostToLinkedInAsync(PostPublicationRequest request, string userId)
        {
            var post = await postService.GetPostById(request.PostId);

            if(post == null)
                throw new NotFoundException("Post not found");

            if(post.UserId != userId)
            {
                throw new UnauthorizedException("User is not the owner of the post");
            }

            // to verify if the user has the platform linked before trying to publish and is the owner of the platform
            var userPlatform = await userPlatformService.GetUserPlatformByIdAsync(request.UserPlatformId, userId);

            if(userPlatform.AccessToken == null || userPlatform.ExternalAccountId == null)
                throw new BadRequestException("User platform does not have an access token or ExternalAccount Id");

            //publication logic

            LinkedInPostRequest linkedInPostRequest = new LinkedInPostRequest
            {
                AccessToken = userPlatform.AccessToken,
                Content = post.Body,
                ExternalAccountId = userPlatform.ExternalAccountId
            };

            await linkedInService.PostTextAsync(linkedInPostRequest);

            PostPublicationResponse response = new PostPublicationResponse
            {
                PostId = post.Id,
                UserPlatformId = userPlatform.Id,
                PublishedAt = DateTime.UtcNow,
                Status = PostPublicationStatus.Published,
                ExternalPostId = null // This would be set to the ID returned by LinkedIn after a successful post, if available
            };

            return response;
        }
    }
}
