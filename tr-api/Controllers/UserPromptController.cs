using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using tr_backend.Helpers;
using tr_core.DTO.UserPrompt.Request;
using tr_core.DTO.UserPrompt.Response;
using tr_core.Services;
using tr_service.Mapping;

namespace tr_backend.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class UserPromptController(IUserPromptService userPromptService, IMapper mapper) : ControllerBase
    {

        [HttpGet("userPrompts")]
        public async Task<PaginatedList<UserPromptResponse>> GetAllUserPrompts([FromQuery] UserPromptQueryParams queryParams)
        {
            var userId = UserHelpers.GetUserIdFromClaims(User);

            var prompts = await userPromptService.GetAllPerUserWithParamsAsync(queryParams, userId);

            return await PaginatedList<UserPromptResponse>.CreateAsync(prompts.AsQueryable(), mapper, queryParams.PageNumber, queryParams.PageSize);
        }

    }
}
