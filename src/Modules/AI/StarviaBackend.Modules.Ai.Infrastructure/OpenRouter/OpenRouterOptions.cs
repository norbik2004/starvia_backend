namespace StarviaBackend.Modules.Ai.Infrastructure.OpenRouter;

// TODO: Same as GoogleAiStudioOptions, but for OpenRouter. The OpenRouter API is a proxy to multiple LLMs (including OpenAI's),
// so it has a different set of options. We may want to merge these two classes in the future,
// but for now we keep them separate because they have different options and are used in different contexts.
internal sealed class OpenRouterOptions
{
    public const string SectionName = "integrations:openrouter";

    /// <summary>OpenRouter speaks the OpenAI protocol, so this is the endpoint the OpenAI library is pointed at.</summary>
    public string BaseUrl { get; set; } = "https://openrouter.ai/api/v1/";

    /// <summary>Secret key from openrouter.ai. Never commit it: supply it through user secrets or the environment.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>The model that answers, in OpenRouter's <c>vendor/model</c> form.</summary>
    public string Model { get; set; } = "openai/gpt-4o-mini";

    /// <summary>Generating text takes far longer than a typical API call, so the default is generous.</summary>
    public int TimeoutSeconds { get; set; } = 120;
}
