using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tr_core.DTO.User.Response
{
    public class UserResponse
    {
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public List<string> Roles { get; set; } = [];
        public int PostsPublished { get; set; }
        public int PostsGenerated { get; set; }
        public bool IsSubscribed { get; set; }
        public string? StripeCustomerId { get; set; } 
    }
}
