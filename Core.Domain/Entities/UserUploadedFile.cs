using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Domain.Entities
{
    public class UserUploadedFile : IAuditable
    {
        [Key]
        public Guid Id { get; set; }

        [ForeignKey(nameof(User))]
        public required string UserId { get; set; }

        public User User { get; set; } = null!;

        public required string FileName { get; set; }

        public required string FilePath { get; set; }
       
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<PostAttachment> PostAttachments { get; set; } = [];
    }
}
