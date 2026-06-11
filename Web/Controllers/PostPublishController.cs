using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Helpers;
using Core.DTO.PostPublication.Request;
using Core.Services;

namespace Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PostPublishController(IPostPublishService postPublishService) : ControllerBase
    {
        [HttpPost("publish/linkedin")]
        public async Task<IActionResult> PublishPostToLinkedIn([FromBody] PublishPostRequest postRequest)
        {
            var userId = UserHelpers.GetUserIdFromClaims(User);

            var post = await postPublishService.PublishPostToLinkedInAsync(postRequest, userId);

            return Ok(post);
        }
    }
}
