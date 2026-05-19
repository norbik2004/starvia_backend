using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tr_core.DTO.LinkedIn.Response
{
    public class LinkedInAccountInfoResponse
    {
        public required string Sub {  get; set; }
        public required string Name { get; set; }
        public required string PFPurl { get; set; }
    }
}
