using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tr_core.DTO.UserPrompt.Request;
using tr_core.Enums;

namespace tr_core.DTO.Gemini.Request
{
    public class GeminiRequest
    {
        public required UserPromptRequest UserPrompt { get; set; }
        public required GeminiModelType Model {  get; set; }
    }
}
