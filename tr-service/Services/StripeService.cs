using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tr_core.DTO.Stripe;
using tr_core.DTO.Stripe.Request;
using tr_core.DTO.Stripe.Response;
using tr_core.Services;
using tr_repository;


namespace tr_service.Services
{
    public class StripeService(StripeClient stripeClient, IUserService userService) : IStripeService
    {

        public async Task<CreateCheckoutSessionResponse> CreateCheckoutSessionAsync(StripeCheckoutDTO stripeCheckout)
        {
            if (string.IsNullOrWhiteSpace(stripeCheckout.request.PriceId) && string.IsNullOrWhiteSpace(stripeCheckout.request.LookupKey))
                throw new ArgumentException("Provide either priceId or lookupKey");

            var priceId = stripeCheckout.request.PriceId;

            if (string.IsNullOrWhiteSpace(priceId))
            {
                var priceService = new PriceService(stripeClient);
                var prices = await priceService.ListAsync(new PriceListOptions
                {
                    LookupKeys = [stripeCheckout.request.LookupKey!],
                    Limit = 1
                });

                priceId = prices.Data.Count > 0 ? prices.Data[0].Id : null;
            }

            if (string.IsNullOrWhiteSpace(priceId))
                throw new ArgumentException("No Stripe price found for the provided lookupKey");

            // Jeśli user ma już customerId w Stripe, przekazujemy go — Stripe nie stworzy duplikatu
            var user = await userService.GetLoggedInUserInfoAsync(stripeCheckout.userId);
            var existingCustomerId = user?.StripeCustomerId;

            var sessionOptions = new SessionCreateOptions
            {
                Mode = stripeCheckout.request.Mode,
                LineItems =
                [
                    new SessionLineItemOptions
                    {
                        Price = priceId,
                        Quantity = 1
                    }
                ],
                SuccessUrl = stripeCheckout.request.SuccessUrl,
                CancelUrl = stripeCheckout.request.CancelUrl,
                // Metadata pozwala nam powiązać subskrypcję z userem w webhooku
                SubscriptionData = new SessionSubscriptionDataOptions
                {
                    Metadata = new Dictionary<string, string>
                    {
                        { "userId", stripeCheckout.userId }
                    }
                }
            };

            // Jeśli mamy już customerId — przekazujemy go, żeby Stripe nie tworzył nowego klienta
            if (!string.IsNullOrWhiteSpace(existingCustomerId))
                sessionOptions.Customer = existingCustomerId;
            else
                sessionOptions.CustomerEmail = stripeCheckout.userEmail;

            var sessionService = new SessionService(stripeClient);
            var session = await sessionService.CreateAsync(sessionOptions);

            return new CreateCheckoutSessionResponse(session.Id, session.Url);
        }

        public async Task<CreatePortalSessionResponse> CreatePortalSessionAsync(string userId, string returnUrl)
        {
            var user = await userService.GetLoggedInUserInfoAsync(userId);

            if (string.IsNullOrWhiteSpace(user.StripeCustomerId))
                throw new InvalidOperationException("User does not have an active Stripe subscription");

            var portalService = new Stripe.BillingPortal.SessionService(stripeClient);
            var portalSession = await portalService.CreateAsync(new Stripe.BillingPortal.SessionCreateOptions
            {
                Customer = user.StripeCustomerId,
                ReturnUrl = returnUrl
            });

            return new CreatePortalSessionResponse(portalSession.Url);
        }

        public async Task HandleWebhookAsync(string json, string stripeSignature)
        {
            // Webhook secret waliduje że request faktycznie pochodzi od Stripe
            var webhookSecret = Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET")
                ?? throw new InvalidOperationException("Missing Stripe webhook secret");

            var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, webhookSecret);

            switch (stripeEvent.Type)
            {
                case EventTypes.CustomerSubscriptionCreated:
                    await HandleSubscriptionActiveAsync((Subscription)stripeEvent.Data.Object);
                    break;
                case EventTypes.CustomerSubscriptionUpdated:
                    await HandleSubscriptionActiveAsync((Subscription)stripeEvent.Data.Object);
                    break;

                case EventTypes.CustomerSubscriptionDeleted:
                    await HandleSubscriptionDeletedAsync((Subscription)stripeEvent.Data.Object);
                    break;
            }
        }

        private async Task HandleSubscriptionActiveAsync(Subscription subscription)
        {
            var userId = subscription.Metadata.GetValueOrDefault("userId");
            if (string.IsNullOrWhiteSpace(userId)) return;

            await userService.SetStripeCustomerId(userId, subscription.CustomerId);
            await userService.UpdateSubsciptionStatus(userId, subscription.Status == "active");
        }

        private async Task HandleSubscriptionDeletedAsync(Subscription subscription)
        {
            var userId = subscription.Metadata.GetValueOrDefault("userId");
            if (string.IsNullOrWhiteSpace(userId)) return;

            await userService.UpdateSubsciptionStatus(userId, subscription.Status == "active");
        }
    }
}
