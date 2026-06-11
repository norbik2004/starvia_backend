using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DTO.UserPrompt.Request;
using Core.DTO.UserPrompt.Response;

namespace Core.Services
{
    public interface IUserPromptService
    {
        Task<List<UserPromptResponse>> GetAllPerUserWithParamsAsync(UserPromptQueryParams queryParams, string userId);
    }
}
