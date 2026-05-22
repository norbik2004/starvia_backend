using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tr_core.DTO.UserPlatform.Request
{
    public class UserPlatformRequest
    {
        public required int PlatformId { get; set; }
        public string? AccessToken { get; set; }
        public string? ExternalAccountId { get; set; }
        public required string AccountUsername { get; set; }
        public required string AccountComment { get; set; }
        public string? ProfilePictureLink { get; set; }
    }
}
