using AutoMapper;
using AutoMapper.QueryableExtensions;
using Core.Application.DTO.LinkedIn.Request;
using Core.Application.DTO.Post.Request;
using Core.Application.DTO.Post.Response;
using Core.Application.DTO.PostPublication.Request;
using Core.Application.DTO.PostPublication.Response;
using Core.Application.Helpers;
using Core.Application.Services;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Repository.Repositories;
using Service.Exceptions;
using System.Linq.Dynamic.Core;

namespace Service.Services
{
    public class PostPublicationService(IMapper mapper, IPostService postService, IUserPlatformService userPlatformService,
        IPostPublicationRepository postPublicationRepository, ILinkedInService linkedInService)
        : BaseHelpers, IPostPublishService
    {
        public async Task<List<PostPublicationResponse>> GetUserPostPublicationsAsync(PostPublicationParamsRequest request,string userId)
        {
            var posts = postPublicationRepository.GetAllAsQueryPerUserIdAsync(userId);

            ValidateQueryParamsDates(request.PublishedBefore, request.PublishedAfter);

            posts = ApplyFilters(request, posts);

            var sortColumn = request.SortBy?.ToString() ?? "Id";
            var direction = request.IsAscending ? "asc" : "desc";

            posts = posts.OrderBy($"{sortColumn} {direction}");

            return await posts
                .ProjectTo<PostPublicationResponse>(mapper.ConfigurationProvider)
                .ToListAsync();
        }

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

            PostPublicationResponse response = new()
            {
                AccountUsername = userPlatform.AccountUsername,
                ExternalPostId = code,
                PlatformType = PlatformType.LinkedIn,
                PostBody = post.Body,
                PublishedAt = postPublicationEntity.PublishedAt,
                Status = PostPublicationStatus.Published,
            };

            return response;
        }

        private static IQueryable<PostPublication> ApplyFilters(PostPublicationParamsRequest request, IQueryable<PostPublication> postPublications)
        {

            if (request.Status != null)
                postPublications = postPublications.Where(p => p.Status == request.Status);

            if(request.PostId != null)
                postPublications = postPublications.Where(p => p.PostId == request.PostId);

            if(request.UserPlatformId != null)
                postPublications = postPublications.Where(p => p.UserPlatformId == request.UserPlatformId);

            if (request.PublishedAfter != null)
                postPublications = postPublications.Where(p => p.PublishedAt >= request.PublishedAfter);

            if (request.PublishedBefore != null)
                postPublications = postPublications.Where(p => p.PublishedAt <= request.PublishedBefore);

            if(request.PublishedOn != null)
                postPublications = postPublications.Where(p => p.UserPlatform.Platform.Type == request.PublishedOn);
        

            return postPublications;
        }

        // TODO : implement scheduling and other platforms
    }
}
