using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tr_core.Entities
{
    public class User : IdentityUser, IAuditable
    {
        public bool IsSubscribed { get; set; }
        public string? StripeCustomerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        //POSTY USERA
        public ICollection<Post> Posts { get; set; } = new List<Post>();
        //PLATFORMY USERA
        public ICollection<UserPlatform> UserPlatforms { get; set; } = new List<UserPlatform>();
        public UserSetting? UserSettings { get; set; }
    }
}