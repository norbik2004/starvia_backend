using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Web.Helpers;
using Core.Domain.Consts;
using Core.Application.DTO.Post.Request;
using Core.Application.DTO.Post.Response;
using Core.Application.DTO.User.Response;
using Core.Application.DTO.UserPlatform.Response;
using Core.Application.Services;
using Service.Exceptions;
using Service.Mapping;
using Service.Services;

namespace Web.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public class PostController(IPostService postService, IMapper mapper) : ControllerBase
    {
        
        [HttpGet("Admin")]
        [Authorize(Roles = Roles.Admin)]
        [ProducesResponseType(typeof(PaginatedList<PostResponse>), StatusCodes.Status200OK)]
        public async Task<PaginatedList<PostResponse>> GetAllPosts([FromQuery] PostPaginatedParamsRequest request)
        {
            var posts = await postService.GetAllPostsAsync(request);

            return await PaginatedList<PostResponse>.CreateAsync(posts.AsQueryable(), mapper, request.PageNumber, request.PageSize);
        }

        [HttpGet]
        [ProducesResponseType(typeof(PaginatedList<PostResponseLong>), StatusCodes.Status200OK)]
        public async Task<PaginatedList<PostResponseLong>> GetAllUserPosts([FromQuery] PostPaginatedParamsRequest request)
        {
            var userId = UserHelpers.GetUserIdFromClaims(User);

            var posts = await postService.GetAllPostsPerUserAsync(request, userId);

            return await PaginatedList<PostResponseLong>.CreateAsync(posts.AsQueryable(), mapper, request.PageNumber, request.PageSize);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(PostResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<PostResponseLong> GetPostById(int id)
        {
            string userId = UserHelpers.GetUserIdFromClaims(User);
            var post = await postService.GetPostByIdLong(id, userId);
            return post;
        }

        [HttpPost()]
        [ProducesResponseType(typeof(PostResponse), StatusCodes.Status200OK)]
        public async Task<PostResponse> CreatePost([FromBody] PostRequest request)
        {
            var userId = UserHelpers.GetUserIdFromClaims(User);

            return await postService.CreatePostAsync(request, userId);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(PostResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<PostResponse> UpdatePost(int id, [FromBody] PostRequest request)
        {
            var userId = UserHelpers.GetUserIdFromClaims(User);

            return await postService.UpdatePostAsync(id, request, userId);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePost(int id)
        {
            var userId = UserHelpers.GetUserIdFromClaims(User);
            await postService.DeletePost(id, userId);
            return NoContent();
        }
    }
}
