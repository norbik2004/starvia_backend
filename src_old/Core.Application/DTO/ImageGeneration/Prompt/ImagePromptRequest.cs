using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.DTO.ImageGeneration.Prompt
{
    public class ImagePromptRequest
    {
        public required int ConversationId { get; set; }
        public required string Prompt { get; set; }
    }
}
