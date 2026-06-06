using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tr_core.DTO.UserPrompt.Response
{
    public class UserPromptResponse
    {
        public required int Id { get; set; }
        public required string Prompt { get; set; }
        public required string Response { get; set; }
        public required int PostId { get; set; }
        public required DateTime CreatedAt { get; set; }
    }
}
