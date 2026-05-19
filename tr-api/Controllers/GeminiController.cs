using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tr_backend.Helpers;
using tr_core.DTO.Gemini;
using tr_core.DTO.Gemini.Request;
using tr_core.Enums;
using tr_core.Services.Gemini;

namespace tr_backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class GeminiController(IGeminiService geminiService) : ControllerBase
    {
        [HttpPost("ask-gemini")]
        public async Task<GeminiResponse> TestGemini( [FromForm] GeminiRequest request)
        {
            var userId = UserHelpers.GetUserIdFromClaims(User);
            return await geminiService.SendRequestToGemini(userId, request);
        }
    }
}
