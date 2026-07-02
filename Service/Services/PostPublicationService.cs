using AutoMapper;
using Core.Application.DTO.LinkedIn.Request;
using Core.Application.DTO.PostPublication.Request;
using Core.Application.DTO.PostPublication.Response;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Application.Helpers;
using Core.Infrastructure.Repositories;
using Core.Application.Services;
using Service.Exceptions;

namespace Service.Services
{
    public class PostPublicationService(IMapper mapper, IPostService postService, IUserPlatformService userPlatformService,
        IPostPublicationRepository postPublicationRepository, ILinkedInService linkedInService) : BaseHelpers, IPostPublishService
    {
        public async Task<PostPublicationResponse> PublishPostToLinkedInAsync(PublishPostRequest request, string userId)
        {
            var post = await postService.GetPostByIdLong(request.PostId, userId);

            if(post.Body == null)
                throw new BadRequestException("Post body cannot be null");

            // to verify if the user has the platform linked before trying to publish and is the owner of the platform
            var userPlatform = await userPlatformService.GetUserPlatformByIdAsync(request.UserPlatformId, userId);

            if(userPlatform.AccessToken == null || userPlatform.ExternalAccountId == null)
                throw new BadRequestException("User platform does not have an access token or ExternalAccount Id");

            //publication logic

            string? code;

            if (post.Attachments != null && post.Attachments.Count > 0)
            {
                LinkedInPostWithMediaRequest linkedInPostWithMediaRequest = new()
                {
                    AccessToken = userPlatform.AccessToken,
                    Content = post.Body,
                    ExternalAccountId = userPlatform.ExternalAccountId,
                    Attachments = post.Attachments
                };

                code = await linkedInService.PostTextWithMediaAsync(linkedInPostWithMediaRequest, userId);
            }
            else
            {
                LinkedInPostRequest linkedInPostRequest = new()
                {
                    AccessToken = userPlatform.AccessToken,
                    Content = post.Body,
                    ExternalAccountId = userPlatform.ExternalAccountId
                };

                code = await linkedInService.PostTextAsync(linkedInPostRequest);
            }

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
