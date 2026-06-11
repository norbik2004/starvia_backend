using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    }
}
