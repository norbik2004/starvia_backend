using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Application.DTO.Stripe;
using Core.Application.DTO.Stripe.Request;
using Core.Application.DTO.Stripe.Response;

namespace Core.Application.Services
{
    public interface IStripeService
    {
        Task<CreateCheckoutSessionResponse> CreateCheckoutSessionAsync(string userId);
        Task<CreatePortalSessionResponse> CreatePortalSessionAsync(string userId, string returnUrl);
        Task HandleWebhookAsync(string json, string stripeSignature);
    }
}
