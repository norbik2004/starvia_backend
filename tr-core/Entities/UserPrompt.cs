using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tr_core.Entities
{
    public class UserPrompt : BaseEntity, IAuditable
    {
        public required string Prompt { get; set; }
        public required User User { get; set; }

        [ForeignKey(nameof(User))]
        public required string UserId { get; set; }
        public required Post Post { get; set; }

        [ForeignKey(nameof(Post))]
        public required int PostId { get; set; }
    }
}
