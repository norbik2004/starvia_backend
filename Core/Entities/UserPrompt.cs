using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class UserPrompt : BaseEntity, IAuditable
    {
        public required string Prompt { get; set; }
        public required string Response { get; set; }
        public User User { get; set; }

        [ForeignKey(nameof(User))]
        public required string UserId { get; set; }
        public Post Post { get; set; }

        [ForeignKey(nameof(Post))]
        public int? PostId { get; set; }
    }
}
