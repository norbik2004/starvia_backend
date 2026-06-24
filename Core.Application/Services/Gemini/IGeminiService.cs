using Core.Application.DTO.UserPrompt.Request;
using Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.Services.Gemini
{
    public interface IGeminiService
    {
        public Task<string> AskAiPostScope(string userId, UserPromptRequest request);
    }
}
