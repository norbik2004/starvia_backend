using AutoMapper;
using Core.DTO.LinkedIn.Request;
using Core.DTO.PostPublication.Request;
using Core.DTO.PostPublication.Response;
using Core.Entities;
using Core.Enums;
using Core.Helpers;
using Core.Repositories;
using Core.Services;
using Service.Exceptions;

namespace Service.Services
{
    public class PostPublicationService(IMapper mapper, IPostService postService, IUserPlatformService userPlatformService,
        IPostPublicationRepository postPublicationRepository, ILinkedInService linkedInService) : BaseHelpers, IPostPublishService
    {
        public async Task<PostPublicationResponse> PublishPostToLinkedInAsync(PublishPostRequest request, string userId)
        {
            var post = await postService.GetUserPostById(request.PostId, userId);

            if(post == null)
                throw new NotFoundException("Post not found");

            if(post.Body == null)
                throw new BadRequestException("Post body cannot be null");

            if (post.UserId != userId)
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
            

            var code = await linkedInService.PostTextAsync(linkedInPostRequest);

            var postPublicationEntity = mapper.Map<PostPublication>(request);

            postPublicationEntity.Status = PostPublicationStatus.Published;
            postPublicationEntity.PublishedAt = DateTime.UtcNow;
            postPublicationEntity.ExternalPostId = code;

            await postPublicationRepository.AddAsync(postPublicationEntity);
            await postPublicationRepository.SaveChangesAsync();

            var response = mapper.Map<PostPublicationResponse>(postPublicationEntity);

            return response;
        }

        // TODO : implement scheduling and other platforms
    }
}
