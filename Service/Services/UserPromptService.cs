using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Core.Application.DTO.UserPrompt.Request;
using Core.Application.DTO.UserPrompt.Response;
using Core.Infrastructure.Repositories;
using Core.Application.Services;
using System.Linq.Dynamic.Core;
using Core.Application.Helpers;
namespace Service.Services
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
