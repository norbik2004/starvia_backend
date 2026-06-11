using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DTO.UserPrompt.Request;
using Core.Enums;

namespace Core.DTO.Gemini.Request
{
    public class GeminiRequest
    {
        public required UserPromptRequest UserPrompt { get; set; }
        public required GeminiModelType Model {  get; set; }
    }
}
