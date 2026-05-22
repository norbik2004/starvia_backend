using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tr_core.DTO.Gemini;
using tr_core.DTO.Gemini.Request;
using tr_core.Enums;

namespace tr_core.Services.Gemini
{
    public interface IGeminiService
    {
        public Task<GeminiResponse> SendRequestToGemini(string userId, GeminiRequest request);
    }
}
