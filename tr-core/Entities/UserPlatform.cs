using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tr_core.Entities
{
    public class UserPlatform : BaseEntity, IAuditable
    {
        [ForeignKey(nameof(User))]
        public string UserId { get; set; } = null!;
        public User User { get; set; } = null!;

        [ForeignKey(nameof(Platform))]
        public int PlatformId { get; set; }
        public Platform Platform { get; set; } = null!;

        // dane do API
        public string? AccessToken { get; set; }
        public string? ExternalAccountId { get; set; }
        public required string AccountUsername { get; set; }
        public required string AccountComment { get; set; }
        public string? ProfilePictureLink { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<PostPublication> PostPublications { get; set; } = new List<PostPublication>();
    }
}
