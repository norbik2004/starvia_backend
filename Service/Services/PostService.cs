using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;
using System.Threading.Tasks;
using Core.Application.DTO.Post.Request;
using Core.Application.DTO.Post.Response;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Application.Helpers;
using Core.Infrastructure.Repositories;
using Core.Application.Services;
using Service.Exceptions;

namespace Service.Services
{
    public class PostService(IPostRepository postRepository, IUserPromptRepository userPromptRepository,
        IPostAttachmentRepository postAttachmentRepository,
        IMapper mapper) : BaseHelpers, IPostService
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

            foreach(var userPrompt in post.User.UserPrompts)
            {
                userPromptRepository.Remove(userPrompt);
            }

            await userPromptRepository.SaveChangesAsync();

            foreach (var postAttachment in post.Attachments)
            {
                postAttachmentRepository.Remove(postAttachment);
            }

            await postAttachmentRepository.SaveChangesAsync();

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

            var sortColumn = request.SortBy?.ToString() ?? "Id";
            var direction = request.IsAscending ? "asc" : "desc";

            posts = posts.OrderBy($"{sortColumn} {direction}");

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
                posts = posts.Where(p => p.Title.Contains(request.TitleContains, StringComparison.CurrentCultureIgnoreCase));

            if (request.BodyContains != null)
                posts = posts.Where(p => p.Body != null && p.Body.Contains(request.BodyContains, StringComparison.CurrentCultureIgnoreCase));

            return posts;
        }

        public async Task<PostResponseLong> GetPostByIdLong(int postId, string userId)
        {
            var post = await postRepository.GetByIdAsync(postId.ToString());

            if (post == null)
                throw new BadRequestException("Post was not found");

            if (post.UserId != userId)
                throw new UnauthorizedException("User is not the owner of the post");

            return mapper.Map<PostResponseLong>(post);
        }
    }
}
