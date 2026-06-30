using Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.DTO.UserPrompt.Request
{
    public class UserPromptRequest
    {
        [MaxLength(300)]
        public required string Prompt { get; set; }
        public required int PostId { get; set; }
        public bool? IncludePostText { get; set; }
        public required GeminiConversationType ConversationType { get; set; }
    }
}
