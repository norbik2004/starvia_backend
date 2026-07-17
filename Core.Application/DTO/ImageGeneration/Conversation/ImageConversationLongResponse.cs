using Core.Application.DTO.ImageGeneration.Prompt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.DTO.ImageGeneration.Conversation
{
    public class ImageConversationLongResponse
    {
        public string? Title { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<ImagePromptResponse> ImagePromptResponses { get; set; } = [];
    }
}
