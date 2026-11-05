namespace StarviaBackend.Modules.Ai.Infrastructure.GoogleAiStudio;

// TODO: make it possible to change the model per request, so we can use Gemini 1.5 for short answers and Gemini 3.5 for long ones.
internal sealed class GoogleAiStudioOptions
{
    public const string SectionName = "integrations:googleaistudio";

    /// <summary>
    /// Secret key from Google AI Studio (aistudio.google.com/apikey). Never commit it: supply it through user
    /// secrets or the environment (<c>integrations__googleaistudio__ApiKey</c>).
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>The Gemini model that writes the texts, as named by the Gemini API (for example <c>gemini-3.5-flash-lite</c>).</summary>
    public string Model { get; set; } = "gemini-3.5-flash-lite";

    /// <summary>Generating text takes far longer than a typical API call, so the default is generous.</summary>
    public int TimeoutSeconds { get; set; } = 120;
}
