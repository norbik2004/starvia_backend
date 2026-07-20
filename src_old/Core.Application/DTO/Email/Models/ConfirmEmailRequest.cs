using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.DTO.Email.Models
{
    public class ConfirmEmailRequest : BaseEmailRequest
    {
        public required string Token { get; set; }

        public override string Subject => "Confirm your email - Starvia Team";
        public required string UserId { get; set; }
    }
}
