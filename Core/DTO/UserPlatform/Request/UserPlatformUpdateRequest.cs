using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTO.UserPlatform.Request
{
    public class UserPlatformUpdateRequest
    {
        public string? AccessToken { get; set; }
        public string? ExternalAccountId { get; set; }
        public required string AccountUsername { get; set; }
        public required string AccountComment { get; set; }
    }
}
