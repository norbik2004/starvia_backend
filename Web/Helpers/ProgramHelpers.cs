using dotenv.net;
using Google.GenAI;
using Stripe;
using Core.Application.DTO.Stripe;
using Core.Application.Services.Email;
using Service.Email;
using Service.Exceptions;
using Service.Gemini;
using Service.LinkedIn;

namespace Web.Helpers
{
    public static class ProgramHelpers
    {
        public static void AddSingletons(WebApplicationBuilder builder)
        {
            DotEnv.Load();

            var stripeApiKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY");
            var stripeWebhookKey = Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET");
            var stripePriceId = Environment.GetEnvironmentVariable("STRIPE_PRICE_ID");
            var geminiApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
            var linkedinClientId = Environment.GetEnvironmentVariable("LINKEDIN_CLIENT_ID");
            var linkedinClientSecret = Environment.GetEnvironmentVariable("LINKEDIN_CLIENT_SECRET");
            var linkedinRedirect = Environment.GetEnvironmentVariable("LINKEDIN_REDIRECT_URI");
            
            
            if(string.IsNullOrWhiteSpace(stripeApiKey))
                throw new NotFoundException("Missing STRIPE_SECRET_KEY in environmental variables");
            if(string.IsNullOrWhiteSpace(stripeWebhookKey))
                throw new NotFoundException("Missing STRIPE_WEBHOOK_SECRET in environmental variables");
            if(string.IsNullOrWhiteSpace(geminiApiKey))
                throw new NotFoundException("Missing GEMINI_API_KEY in environmental variables");
            if(string.IsNullOrWhiteSpace(linkedinClientId))
                throw new NotFoundException("Missing LINKEDIN_CLIENT_ID in environmental variables");
            if(string.IsNullOrWhiteSpace(linkedinClientSecret))
                throw new NotFoundException("Missing LINKEDIN_CLIENT_SECRET in environmental variables");
            if(string.IsNullOrWhiteSpace(linkedinRedirect))
                throw new NotFoundException("Missing LINKEDIN_REDIRECT_URI in environmental variables");
            if (string.IsNullOrWhiteSpace(stripePriceId))
                throw new NotFoundException("Missing STRIPE_PRICE_ID in environmental variables");

            //STRIPE
            builder.Services.AddSingleton<StripeClient>(_ =>
            {
                return new StripeClient(stripeApiKey);
            });

            //GEMINI
            builder.Services.AddSingleton<Client>(sp =>
            {
                return new Client(apiKey: geminiApiKey);
            });

            //LinkedInConfig
            var linkedInConfig = new LinkedInConfig
            {
                ClientId = linkedinClientId,
                ClientSecret = linkedinClientSecret,
                RedirectUri = linkedinRedirect
            };

            //StripeConfig
            var stripeConfig = new StripeConfig
            {
                ApiKey = stripeApiKey,
                WebhookSecret = stripeWebhookKey,
                PriceId = stripePriceId
            };


            builder.Services.AddAuthentication()
                .AddLinkedIn(options =>
                {
                    options.SaveTokens = true;
                    options.ClientId = linkedinClientId;
                    options.ClientSecret = linkedinClientSecret;
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("email");
                    options.Scope.Add("w_member_social");
                    options.CallbackPath = new PathString("/signin-linkedin-mw-callback");
                });

            builder.Services.AddSingleton(linkedInConfig);
            builder.Services.AddSingleton(stripeConfig);
            builder.Services.AddSingleton<GeminiLlMConfig>();
            builder.Services.AddSingleton<IEmailSender, EmailSender>();
        }
    }
}
