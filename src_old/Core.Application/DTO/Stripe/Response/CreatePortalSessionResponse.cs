using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Application.DTO.Stripe.Response
{
    public sealed record CreatePortalSessionResponse(
        string Url
    );
}
