using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tr_core.Enums;

namespace tr_core.DTO.Email.Models
{
    public abstract class BaseEmailRequest
    {
        public required string To { get; set; }
        public abstract string Subject { get; }
        public required EmailForm EmailType { get; set; }
    }
}
