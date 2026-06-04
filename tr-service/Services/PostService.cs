using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tr_core.DTO.Post.Request;
using tr_core.DTO.Post.Response;
using tr_core.Entities;
using tr_core.Enums;
using tr_core.Helpers;
using tr_core.Repositories;
using tr_core.Services;
using tr_service.Exceptions;

namespace tr_service.Services
{
    public class PostService(IPostRepository postRepository, IMapper mapper) : BaseHelpers, IPostService
    {
        public async Task<PostResponse> CreatePostAsync(PostRequest request, string userId)
        {
            var postEntity = mapper.Map<Post>(request);
            postEntity.UserId = userId;
            postEntity.Status = PostStatus.Draft;

            await postRepository.AddAsync(postEntity);
            await postRepository.SaveChangesAsync();
            
            return mapper.Map<PostResponse>(postEntity);
        }

        public async Task DeletePost(int postId, string userId)
        {
            var post = await postRepository.GetByIdAsync(postId.ToString()) ?? 
                throw new NotFoundException("Post was not found");

            if(post.UserId != userId)
                throw new UnauthorizedException("User is not the owner of the post");

            postRepository.Remove(post);
            await postRepository.SaveChangesAsync();
        }

        public async Task<List<PostResponse>> GetAllPostsAsync(PostPaginatedParamsRequest request)
        {
            var posts = postRepository.GetAllAsQueryAsync();

            ValidateQueryParamsDates(request.CreatedBefore, request.CreatedAfter);

            posts = ApplyFilters(request, posts);

            return await posts
                .ProjectTo<PostResponse>(mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<List<PostResponse>> GetAllPostsPerUserAsync(PostPaginatedParamsRequest request, string userId)
        {
            var posts = postRepository.GetAllAsQueryPerUserIdAsync(userId);

            ValidateQueryParamsDates(request.CreatedBefore, request.CreatedAfter);

            posts = ApplyFilters(request, posts);

            return await posts
                .ProjectTo<PostResponse>(mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<PostResponse> GetUserPostById(int postId, string userId)
        {
            var post = await postRepository.GetByIdAsync(postId.ToString());

            if (post == null)
                throw new BadRequestException("Post was not found");

            if (post.UserId != userId)
                throw new UnauthorizedException("User is not the owner of the post");

            return mapper.Map<PostResponse>(post);
        }

        public async Task<PostResponse> UpdatePostAsync(int postId, PostRequest request, string userId)
        {
            var post = await postRepository.GetByIdAsync(postId.ToString()) ??
                throw new NotFoundException("Post was not found");

            if (post.UserId != userId)
                throw new UnauthorizedException("User is not the owner of the post");

            mapper.Map(request, post);

            postRepository.Update(post);
            await postRepository.SaveChangesAsync();

            return mapper.Map<PostResponse>(post);
        }

        private static IQueryable<Post> ApplyFilters(PostPaginatedParamsRequest request, IQueryable<Post> posts)
        {
            if (request.CreatedAfter != null)
                posts = posts.Where(p => p.CreatedAt >= request.CreatedAfter);

            if (request.CreatedBefore != null)
                posts = posts.Where(p => p.CreatedAt <= request.CreatedBefore);

            if (request.Status != null)
                posts = posts.Where(p => p.Status == request.Status);

            if (request.UserId != null)
                posts = posts.Where(p => p.UserId == request.UserId);

            if (request.PublishedOn != null)
                posts = posts.Where(p => p.PostPublications.Any(p => p.UserPlatform.Platform.Type == request.PublishedOn));

            if (request.HasPublication == true)
            {
                posts = posts.Where(p => p.PostPublications.Any());
            }
            else if (request.HasPublication == false)
            {
                posts = posts.Where(p => !p.PostPublications.Any());
            }

            if (request.TitleContains != null)
                posts = posts.Where(p => p.Title.Contains(request.TitleContains));

            if (request.BodyContains != null)
                posts = posts.Where(p => p.Body != null && p.Body.Contains(request.BodyContains));

            return posts;
        }
    }
}
