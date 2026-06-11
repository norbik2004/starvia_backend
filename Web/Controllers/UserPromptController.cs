using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Web.Helpers;
using Core.Application.DTO.UserPrompt.Request;
using Core.Application.DTO.UserPrompt.Response;
using Core.Application.Services;
using Service.Mapping;

namespace Web.Controllers
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
