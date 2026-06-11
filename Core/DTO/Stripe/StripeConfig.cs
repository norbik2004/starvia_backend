using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTO.Stripe
{
    public class StripeConfig
    {
        public required string ApiKey { get; set; }
        public required string WebhookSecret { get; set; }
        public required string PriceId { get; set; }
    }
}
