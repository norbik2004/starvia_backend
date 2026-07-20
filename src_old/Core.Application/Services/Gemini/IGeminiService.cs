using Core.Application.DTO.ImageGeneration.Prompt;
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
        public Task<ImagePromptResponse> GenerateImage(string userId, ImagePromptRequest request);
        public Task<string> GenerateUserMimicConfig(string userId, string userTexts);
    }
}
