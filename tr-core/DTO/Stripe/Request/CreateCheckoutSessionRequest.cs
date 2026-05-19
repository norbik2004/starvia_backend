namespace tr_core.DTO.Stripe.Request
{
    public sealed class CreateCheckoutSessionRequest
    {
        /// <summary>Stripe Price ID (preferred) e.g. price_123</summary>
        public string? PriceId { get; init; }

        /// <summary>Stripe Price lookup key (alternative) e.g. "pro_monthly"</summary>
        public string? LookupKey { get; init; }

        /// <summary>Absolute URL the customer returns to after success.</summary>
        public required string SuccessUrl { get; init; }

        /// <summary>Absolute URL the customer returns to after cancel.</summary>
        public required string CancelUrl { get; init; }

        /// <summary>checkout mode: "subscription" or "payment"</summary>
        public string Mode { get; init; } = "subscription";
    }
}