using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DTO.Stripe;
using Core.DTO.Stripe.Request;
using Core.DTO.Stripe.Response;

namespace Core.Services
{
    public interface IStripeService
    {
        Task<CreateCheckoutSessionResponse> CreateCheckoutSessionAsync(string userId);
        Task<CreatePortalSessionResponse> CreatePortalSessionAsync(string userId, string returnUrl);
        Task HandleWebhookAsync(string json, string stripeSignature);
    }
}
