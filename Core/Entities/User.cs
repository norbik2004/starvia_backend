using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class User : IdentityUser, IAuditable
    {
        public bool IsSubscribed { get; set; }
        public string? StripeCustomerId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        //POSTY USERA
        public ICollection<Post> Posts { get; set; } = [];
        //PLATFORMY USERA
        public ICollection<UserPlatform> UserPlatforms { get; set; } = [];
        public UserSetting? UserSettings { get; set; }
        public ICollection<UserPrompt> UserPrompts { get; set; } = [];
    }
}