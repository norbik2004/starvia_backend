using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Entities
{
    public class ImagePrompt : BaseEntity, IAuditable
    {
        public required string Prompt { get; set; }
        public ImagePromptFile? ImagePromptFile { get; set; }

        [ForeignKey(nameof(Conversation))]
        public int ConversationId { get; set; }
        public ImageConversation Conversation { get; set; }

    }
}
