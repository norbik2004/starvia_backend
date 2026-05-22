using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tr_core.DTO.LinkedIn.Request
{
    public class LinkedInPostRequest
    {
        public required string AccessToken { get; set; }
        public required string ExternalAccountId { get; set; }
        public required string Content { get; set; }
    }
}
