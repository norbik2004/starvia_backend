using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tr_backend.Helpers;
using tr_core.DTO.PostPublication.Request;
using tr_core.Services;

namespace tr_backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PostPublishController(IPostPublishService postPublishService) : ControllerBase
    {
        [HttpPost("publish/linkedin")]
        public async Task<IActionResult> PublishPostToLinkedIn([FromBody] PostPublicationRequest postRequest)
        {
            var userId = UserHelpers.GetUserIdFromClaims(User);

            var post = await postPublishService.PublishPostToLinkedInAsync(postRequest, userId);

            return Ok(post);
        }
    }
}
