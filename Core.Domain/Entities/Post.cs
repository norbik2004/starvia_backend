using System.ComponentModel.DataAnnotations.Schema;
using Core.Domain.Enums;

namespace Core.Domain.Entities
{
    public class Post : BaseEntity, IAuditable
    {
        [ForeignKey(nameof(User))]
        public string UserId { get; set; } = null!;
        public User User { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? PromptText { get; set; }
        public string? Body { get; set; }
        public PostStatus Status { get; set; }
        public ICollection<PostPublication> PostPublications { get; set; } = [];
        public ICollection<PostAttachment> Attachments { get; set; } = [];
    }
}
