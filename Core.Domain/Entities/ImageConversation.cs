using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Domain.Entities
{
    public class ImageConversation : BaseEntity, IAuditable
    {
        public string? Title { get; set; }
        public User User { get; set; }

        [ForeignKey(nameof(User))]
        public required string UserId { get; set; }
        public ICollection<ImagePrompt> ImagePrompts { get; set; } = [];
    }
}
