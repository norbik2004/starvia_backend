using dotenv.net;
using Google.GenAI;
using Stripe;
using tr_service.Gemini;
using tr_service.LinkedIn;

namespace tr_backend.Helpers
{
    public class ProgramHelpers()
    {
        public static void AddSingletons(WebApplicationBuilder builder)
        {
            DotEnv.Load();

            var stripeApiKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY");
            var stripeWebhookKey = Environment.GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET");
            var geminiApiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
            var linkedinClientId = Environment.GetEnvironmentVariable("LINKEDIN_CLIENT_ID");
            var linkedinClientSecret = Environment.GetEnvironmentVariable("LINKEDIN_CLIENT_SECRET");
            var linkedinRedirect = Environment.GetEnvironmentVariable("LINKEDIN_REDIRECT_URI");
            
            if(string.IsNullOrWhiteSpace(stripeApiKey))
                throw new Exception("Missing STRIPE_SECRET_KEY in environmental variables");
            if(string.IsNullOrWhiteSpace(stripeWebhookKey))
                throw new Exception("Missing STRIPE_WEBHOOK_SECRET in environmental variables");
            if(string.IsNullOrWhiteSpace(geminiApiKey))
                throw new Exception("Missing GEMINI_API_KEY in environmental variables");
            if(string.IsNullOrWhiteSpace(linkedinClientId))
                throw new Exception("Missing LINKEDIN_CLIENT_ID in environmental variables");
            if(string.IsNullOrWhiteSpace(linkedinClientSecret))
                throw new Exception("Missing LINKEDIN_CLIENT_SECRET in environmental variables");
            if(string.IsNullOrWhiteSpace(linkedinRedirect))
                throw new Exception("Missing LINKEDIN_REDIRECT_URI in environmental variables");
            
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

            builder.Services.AddSingleton(linkedInConfig);
            builder.Services.AddSingleton<GeminiLLMConfig>();

        }
    }
}
