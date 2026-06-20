using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.Helpers;
using Core.Application.DTO.Gemini;
using Core.Application.DTO.Gemini.Request;
using Core.Domain.Enums;
using Core.Application.Services.Gemini;

namespace Web.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class GeminiController(IGeminiService geminiService) : ControllerBase
    {
        [HttpPost("generate-post")]
        public async Task<string> GeneratePost( [FromForm] GeminiRequest request)
        {
            var userId = UserHelpers.GetUserIdFromClaims(User);
            return await geminiService.GeneratePost(userId, request);
        }

        [HttpPost("ask-gemini")]
        public async Task<string> AskGemini([FromForm] GeminiRequest request)
        {
            var userId = UserHelpers.GetUserIdFromClaims(User);
            return await geminiService.AskGemini(userId, request);
        }
    }
}
