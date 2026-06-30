using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.DTO.UserPlatform.Request
{
    public class UserPlatformUpdateRequest
    {
        public required string AccountUsername { get; set; }
        public required string AccountComment { get; set; }
    }
}
