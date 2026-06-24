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
        [MaxLength(150)]
        public required string Prompt { get; set; }
        public required int PostId { get; set; }
        public required GeminiConversationType ConversationType { get; set; }
    }
}
