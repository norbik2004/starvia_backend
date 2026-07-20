using Core.Application.DTO.ImageGeneration.Conversation;
using Core.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Minio.DataModel.Result;
using Web.Helpers;

namespace Web.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ImageConversationController(IImageConversationService imageConversationService) : ControllerBase
    {
        [HttpGet("{id:int}")]
        public async Task<ImageConversationLongResponse> GetImageConversationById(int id)
        {
            var userId = UserHelpers.GetUserIdFromClaims(User);

            return await imageConversationService.GetImageConversationLongPerId(userId, id);
        }

        [HttpGet]
        public async Task<List<ImageConversationResponse>> GetAllConversationsPerUser()
        {
            var userId = UserHelpers.GetUserIdFromClaims(User);
            return await imageConversationService.GetAllConversationPerUserAsync(userId);
        }

        [HttpPost]
        public async Task<ImageConversationResponse> AddImageConversation([FromBody] string? title)
        {
            var userId = UserHelpers.GetUserIdFromClaims(User);
            return await imageConversationService.AddImageConversationAsync(userId, title);
        }
    }
}
