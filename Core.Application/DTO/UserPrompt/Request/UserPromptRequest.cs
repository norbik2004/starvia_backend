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
        [MaxLength(100)]
        public required string Prompt { get; set; }
        public required int PostId { get; set; }
    }
}
