using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tr_core.Enums;

namespace tr_core.Entities
{
    public class PostPublication : BaseEntity, IAuditable
    {
        [ForeignKey(nameof(Post))]
        public int PostId { get; set; }
        public Post Post { get; set; } = null!;

        [ForeignKey(nameof(UserPlatform))]
        public int UserPlatformId { get; set; }
        public UserPlatform UserPlatform { get; set; }
        public required PostPublicationStatus Status { get; set; }
        public DateTime? PublishedAt { get; set; }
        public string? ExternalPostId { get; set; }
    }
}
