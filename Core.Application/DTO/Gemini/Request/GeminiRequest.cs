using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Application.DTO.UserPrompt.Request;
using Core.Domain.Enums;

namespace Core.Application.DTO.Gemini.Request
{
    public class GeminiRequest
    {
        public required UserPromptRequest UserPrompt { get; set; }
        public required GeminiModelType Model {  get; set; }
    }
}
