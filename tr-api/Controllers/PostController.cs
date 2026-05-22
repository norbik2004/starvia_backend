using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using tr_backend.Helpers;
using tr_core.Consts;
using tr_core.DTO.Post.Request;
using tr_core.DTO.Post.Response;
using tr_core.DTO.User.Response;
using tr_core.DTO.UserPlatform.Response;
using tr_core.Services;
using tr_service.Exceptions;
using tr_service.Mapping;
using tr_service.Services;

namespace tr_backend.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PostController(IPostService postService, IMapper mapper) : ControllerBase
    {
        [Authorize(Roles = Roles.Admin)]
        [HttpGet("posts")]
        [ProducesResponseType(typeof(PaginatedList<PostResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<PaginatedList<PostResponse>> GetAllPosts([FromQuery] PostPaginatedParamsRequest request)
        {
            var posts = await postService.GetAllPostsAsync(request);

            return await PaginatedList<PostResponse>.CreateAsync(posts.AsQueryable(), mapper, request.PageIndex, request.PageSize);
        }

        [HttpGet("myPosts")]
        [ProducesResponseType(typeof(PaginatedList<PostResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<PaginatedList<PostResponse>> GetAllUserPosts([FromQuery] PostPaginatedParamsRequest request)
        {
            var userId = UserHelpers.GetUserIdFromClaims(User);

            var posts = await postService.GetAllPostsPerUserAsync(request, userId);

            return await PaginatedList<PostResponse>.CreateAsync(posts.AsQueryable(), mapper, request.PageIndex, request.PageSize);
        }

        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(PostResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<PostResponse> GetPostById(int id)
        {
            string userId = UserHelpers.GetUserIdFromClaims(User);
            var post = await postService.GetUserPostById(id, userId);
            return post;
        }

        [HttpPost("add")]
        [ProducesResponseType(typeof(PostResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<PostResponse> CreatePost([FromBody] PostRequest request)
        {
            var userId = UserHelpers.GetUserIdFromClaims(User);

            return await postService.CreatePostAsync(request, userId);
        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(PostResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<PostResponse> UpdatePost(int id, [FromBody] PostRequest request)
        {
            var userId = UserHelpers.GetUserIdFromClaims(User);

            return await postService.UpdatePostAsync(id, request, userId);
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePost(int id)
        {
            var userId = UserHelpers.GetUserIdFromClaims(User);
            await postService.DeletePost(id, userId);
            return NoContent();
        }
    }
}
