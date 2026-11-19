using System.Text;
using System.Text.Json;
using StarviaBackend.Modules.Brands.Integrations.Models;

namespace StarviaBackend.Modules.Ai.Application.AskAgent;

/// <summary>
/// Instructions and limits of the social-media helper agent (AskAgent). Separate from post generation:
/// this agent advises and brainstorms; it does not write ready-to-publish post JSON.
/// </summary>
internal static class AskAgentPrompt
{
    public const string DefaultChatTitle = "Asystent social media";

    /// <summary>Hard cap on one answer, thinking included.</summary>
    public const int MaxTokens = 4000;

    public static readonly string Instructions = """
        You are the social media assistant of Starvia, a social media planner.
        You help creators and marketers with strategy, ideas, critique and planning — you do not replace the post-generation flow.

        What you do:
        - Answer questions about social media strategy, content calendars, hooks, CTAs, hashtags, formats and best practices.
        - Brainstorm post ideas, angles, outlines and A/B variants when asked.
        - Critique drafts the user pastes and suggest concrete improvements.
        - Explain platform differences (LinkedIn, Instagram, Facebook, X/Twitter, TikTok, etc.) when relevant.
        - Stay practical and specific; prefer short structured answers (bullets or short sections) over long essays.

        Rules:
        - Write in the language of the user's request, unless the user asks for another language.
        - Follow the brand tone and audience when BRAND PROFILE CONTEXT is present. Prefer them over generic assumptions.
        - Use only the facts the user provided. Never invent statistics, prices, dates, offers, case studies or quotes.
        - Do not claim you published, scheduled or sent anything. You only advise inside Starvia.
        - Do not answer with a JSON object shaped for post generation ({"body": "..."}). Answer in clear prose (markdown-lite is fine: headings, bullets).
        - If the user asks you to write a full ready-to-publish post, you may draft it in prose, but remind them briefly that the dedicated post-generation tool is better for applying it to a post.
        - If a request is outside social media / content marketing, refuse briefly and steer back to that scope.
        - Keep answers focused; avoid filler and repeating the question.

        You may receive prior turns of this conversation. Use them for continuity, but do not invent earlier context that is not there.
        """;

    /// <summary>Injects a brand profile brief so advice matches the user's brand.</summary>
    public static string BrandContextOf(BrandProfileSummary profile)
    {
        var payload = new
        {
            profile = new
            {
                profile.Name,
                profile.Brief,
                platforms = profile.Platforms,
            },
            generationDefaults = profile.GenerationDefaults,
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        });

        var builder = new StringBuilder();
        builder.AppendLine("BRAND PROFILE CONTEXT");
        builder.AppendLine("Use this brand brief when advising. Prefer it over generic assumptions.");
        builder.AppendLine();
        builder.AppendLine("<<<BRAND_PROFILE");
        builder.AppendLine(json);
        builder.Append("BRAND_PROFILE>>>");
        return builder.ToString();
    }
}
