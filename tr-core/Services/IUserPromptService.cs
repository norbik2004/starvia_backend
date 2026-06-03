using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tr_core.DTO.UserPrompt.Request;
using tr_core.DTO.UserPrompt.Response;

namespace tr_core.Services
{
    public interface IUserPromptService
    {
        Task<List<UserPromptResponse>> GetAllPerUserWithParamsAsync(UserPromptQueryParams queryParams, string userId);
    }
}
