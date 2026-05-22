using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tr_core.DTO.Stripe.Request
{
    public sealed class CreatePortalSessionRequest
    {
        /// <summary>Absolute URL the customer returns to after portal.</summary>
        public required string ReturnUrl { get; init; }
    }
}
