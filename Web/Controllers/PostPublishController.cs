using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Helpers;
using Core.Application.DTO.PostPublication.Request;
using Core.Application.Services;
using Service.Mapping;
using Core.Application.DTO.PostPublication.Response;
using AutoMapper;

namespace Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PostPublishController(IPostPublishService postPublishService, IMapper mapper) : ControllerBase
    {
        [HttpPost("publish/linkedin")]
        public async Task<IActionResult> PublishPostToLinkedIn([FromBody] PublishPostRequest postRequest)
        {
            var userId = UserHelpers.GetUserIdFromClaims(User);

            var post = await postPublishService.PublishPostToLinkedInAsync(postRequest, userId);

            return Ok(post);
        }

        [HttpGet]
        public async Task<PaginatedList<PostPublicationResponse>> GetUserPostPublications([FromQuery] PostPublicationParamsRequest request)
        {
            var userId = UserHelpers.GetUserIdFromClaims(User);

            var posts = await postPublishService.GetUserPostPublicationsAsync(request, userId);

            return await PaginatedList<PostPublicationResponse>.CreateAsync(posts.AsQueryable(), mapper, request.PageNumber, request.PageSize);
        }
    }
}
