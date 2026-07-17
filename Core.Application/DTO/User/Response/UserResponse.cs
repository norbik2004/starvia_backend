using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.DTO.User.Response
{
    public class UserResponse
    {
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public List<string> Roles { get; set; } = [];
        public bool IsSubscribed { get; set; }
        public string? StripeCustomerId { get; set; } 
        public bool HasLlmInstructions { get; set; }
    }
}
