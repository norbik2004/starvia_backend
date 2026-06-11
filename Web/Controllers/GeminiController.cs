using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Helpers;
using Core.DTO.Gemini;
using Core.DTO.Gemini.Request;
using Core.Enums;
using Core.Services.Gemini;

namespace Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class GeminiController(IGeminiService geminiService) : ControllerBase
    {
        [HttpPost("generate-post")]
        public async Task GeneratePost( [FromForm] GeminiRequest request)
        {
            var userId = UserHelpers.GetUserIdFromClaims(User);
            await geminiService.GeneratePost(userId, request);
        }

        [HttpPost("ask-gemini")]
        public async Task<GeminiResponse> AskGemini([FromForm] GeminiRequest request)
        {
            var userId = UserHelpers.GetUserIdFromClaims(User);
            return await geminiService.AskGemini(userId, request);
        }
    }
}
