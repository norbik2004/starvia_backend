using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTO.Stripe.Response
{
    public sealed record CreateCheckoutSessionResponse(
        string SessionId,
        string Url
    );
}
