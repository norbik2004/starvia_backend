using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Domain.Entities
{
    public class PostAttachment : BaseEntity, IAuditable
    {
        [ForeignKey(nameof(Post))]
        public int PostId { get; set; }

        public Post Post { get; set; } = null!;

        [ForeignKey(nameof(File))]
        public Guid UserUploadedFileId { get; set; }

        public UserUploadedFile File { get; set; } = null!;
        public int Order { get; set; }
    }
}