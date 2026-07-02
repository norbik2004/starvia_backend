using Core.Application.DTO.PostAttachment.Request;
using Core.Application.DTO.PostAttachment.Response;
using Core.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Helpers;

namespace Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PostAttachmentController(IPostAttachmentService postAttachmentService) : ControllerBase
    {
        [HttpPost]
        public async Task<List<PostAttachmentResponse>> AddPostAttachmentAsync(PostAttachmentRequest request)
        {
            var userId = UserHelpers.GetUserIdFromClaims(User);

            return await postAttachmentService.AddPostAttachmentAsync(request, userId);
        }

        [HttpDelete]
        public async Task<IActionResult> RemovePostAttachmentAsync(int postAttachmentId)
        {
            var userId = UserHelpers.GetUserIdFromClaims(User);

            await postAttachmentService.RemovePostAttachment(postAttachmentId, userId);

            return Ok();
        }

        [HttpPut]
        public async Task<List<PostAttachmentResponse>> UpdatePostAttachmentsOrder(UpdatePostAttachmentOrdersRequest request)
        {
            var userId = UserHelpers.GetUserIdFromClaims(User);

            return await postAttachmentService.UpdatePostAttachmentOrdersAsync(request, userId);
        }
    }
}
