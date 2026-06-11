using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DTO.Gemini;
using Core.DTO.Gemini.Request;
using Core.Enums;

namespace Core.Services.Gemini
{
    public interface IGeminiService
    {
        public Task GeneratePost(string userId, GeminiRequest request);
        public Task<GeminiResponse> AskGemini(string userId, GeminiRequest request);
    }
}
