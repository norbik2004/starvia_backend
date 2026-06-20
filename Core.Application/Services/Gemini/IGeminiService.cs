using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Application.DTO.Gemini;
using Core.Application.DTO.Gemini.Request;
using Core.Domain.Enums;

namespace Core.Application.Services.Gemini
{
    public interface IGeminiService
    {
        public Task<string> GeneratePost(string userId, GeminiRequest request);
        public Task<string> AskGemini(string userId, GeminiRequest request);
    }
}
