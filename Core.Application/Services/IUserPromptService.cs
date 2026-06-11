using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Application.DTO.UserPrompt.Request;
using Core.Application.DTO.UserPrompt.Response;

namespace Core.Application.Services
{
    public interface IUserPromptService
    {
        Task<List<UserPromptResponse>> GetAllPerUserWithParamsAsync(UserPromptQueryParams queryParams, string userId);
    }
}
