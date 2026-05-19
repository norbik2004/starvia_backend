using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tr_core.DTO.Platform.Response;

namespace tr_core.DTO.UserPlatform.Response
{
    public class UserPlatformResponseLong
    {
        public int Id { get; set; }
        public required string AccountUsername { get; set; }
        public string? AccessToken { get; set; }
        public string? ExternalAccountId { get; set; }
        public required string AccountComment { get; set; }
        public string? ProfilePictureLink { get; set; }
        public PlatformResponse Platform { get; set; } = new PlatformResponse();
    }
}
