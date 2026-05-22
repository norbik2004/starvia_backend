using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
using tr_service.Exceptions;


namespace tr_service.Services
{
    public class StripeService(StripeClient stripeClient, StripeConfig stripeConfig, IUserService userService,
         IConfiguration configuration) : IStripeService
    {

        public async Task<CreateCheckoutSessionResponse> CreateCheckoutSessionAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(stripeConfig.PriceId))
                throw new NotFoundException("PriceId key was not found");

            // Jeśli user ma już customerId w Stripe, przekazujemy go — Stripe nie stworzy duplikatu
            var user = await userService.GetLoggedInUserInfoAsync(userId);

            if(user.IsSubscribed)
                throw new BadRequestException("User already has an active subscription");

            var existingCustomerId = user.StripeCustomerId;

            var sessionOptions = new SessionCreateOptions
            {
                Mode = "subscription",
                LineItems =
                [
                    new SessionLineItemOptions
                    {
                        Price = stripeConfig.PriceId,
                        Quantity = 1
                    }
                ],
                SuccessUrl = configuration["Stripe:SuccessUrl"],
                CancelUrl = configuration["Stripe:CancelUrl"],

                // Metadata pozwala nam powiązać subskrypcję z userem w webhooku
                SubscriptionData = new SessionSubscriptionDataOptions
                {
                    Metadata = new Dictionary<string, string>
                    {
                        { "userId", userId }
                    }
                }
            };

            // Jeśli mamy już customerId — przekazujemy go, żeby Stripe nie tworzył nowego klienta
            if (!string.IsNullOrWhiteSpace(existingCustomerId))
                sessionOptions.Customer = existingCustomerId;
            else
                sessionOptions.CustomerEmail = user.Email;

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
            var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, stripeConfig.WebhookSecret);

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
