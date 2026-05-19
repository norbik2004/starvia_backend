using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tr_core.DTO.Stripe;
using tr_core.DTO.Stripe.Request;
using tr_core.DTO.Stripe.Response;

namespace tr_core.Services
{
    public interface IStripeService
    {
        Task<CreateCheckoutSessionResponse> CreateCheckoutSessionAsync(StripeCheckoutDTO stripeCheckout);
        Task<CreatePortalSessionResponse> CreatePortalSessionAsync(string userId, string returnUrl);
        Task HandleWebhookAsync(string json, string stripeSignature);
    }
}
