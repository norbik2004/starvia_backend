using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Helpers;
using Core.Domain.Enums;
using Core.Application.Services.Gemini;
using Core.Application.DTO.UserPrompt.Request;
using Repository.Repositories;
using Core.Application.DTO.ImageGeneration.Prompt;

namespace Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class GeminiController(IGeminiService geminiService) : ControllerBase
    {
        [HttpPost()]
        public async Task<string> AskAiPostScope( [FromForm] UserPromptRequest request)
        {
            var userId = UserHelpers.GetUserIdFromClaims(User);
            return await geminiService.AskAiPostScope(userId, request);
        }

        [HttpPost("image")]
        public async Task<ImagePromptResponse> GenerateImage([FromForm] ImagePromptRequest request)
        {
            var userId = UserHelpers.GetUserIdFromClaims(User);

            return await geminiService.GenerateImage(userId, request);
        }
    }
}
