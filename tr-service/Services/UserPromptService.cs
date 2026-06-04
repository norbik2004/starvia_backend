using AutoMapper;
using Microsoft.EntityFrameworkCore;
using tr_core.DTO.UserPrompt.Request;
using tr_core.DTO.UserPrompt.Response;
using tr_core.Repositories;
using tr_core.Services;
using System.Linq.Dynamic.Core;
using tr_core.Helpers;
namespace tr_service.Services
{
    public class UserPromptService(IUserPromptRepository userPromptRepository, IMapper mapper) : BaseHelpers, IUserPromptService
    {
        public async Task<List<UserPromptResponse>> GetAllPerUserWithParamsAsync(UserPromptQueryParams queryParams, string userId)
        {

            var prompts = userPromptRepository.GetAllAsQueryPerUserIdAsync(userId);

            if(queryParams.PromptContains != null)
            {
                prompts = prompts.Where(p => p.Prompt.Contains(queryParams.PromptContains));
            }
            if(queryParams.CreatedAfter != null)
            {
                prompts = prompts.Where(p => p.CreatedAt >= queryParams.CreatedAfter);
            }
            if (queryParams.CreatedBefore != null)
            {
                prompts = prompts.Where(p => p.CreatedAt <= queryParams.CreatedBefore);
            }

            prompts = prompts.OrderBy($"{queryParams.SortBy} {(queryParams.IsAscending ? "ascending" : "descending")}");

            await prompts.ToListAsync();

            return mapper.Map<List<UserPromptResponse>>(prompts);
        }
    }
}
