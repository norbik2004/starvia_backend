using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tr_core.DTO.Stripe.Request;

namespace tr_core.DTO.Stripe
{
    public class StripeCheckoutDTO
    {
        public required string userId;
        public string? userEmail;
        public required CreateCheckoutSessionRequest request { get; set; }
    }
}
