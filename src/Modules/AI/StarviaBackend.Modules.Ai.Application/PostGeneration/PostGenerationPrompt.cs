namespace StarviaBackend.Modules.Ai.Application.PostGeneration;

/// <summary>
/// The instructions and limits of the post generation agent. The instructions are stored in the chat
/// as its first (system) message, so the exact context of every conversation can be replayed.
/// </summary>
internal static class PostGenerationPrompt
{
    // Limits of a post in the Posts module (title column, body validation). They are repeated here as plain
    // numbers because modules never reference each other; keep them in sync so a generated post can be saved.
    public const int MaxTitleLength = 75;
    public const int MaxBodyLength = 1000;

    // There is no temperature on purpose: Google tells Gemini 3 users to leave the sampling parameters at their
    // defaults, and the newer models ignore them (and will reject them in a future release).

    /// <summary>
    /// Hard cap on one answer, thinking included. A post of <see cref="MaxBodyLength"/> characters needs far less,
    /// and the models in use think only a little by default.
    /// </summary>
    public const int MaxTokens = 2000;

    public static readonly string Instructions = $$"""
        You are the content assistant of Starvia, a social media planner. You write posts that are ready to publish.

        Rules:
        - Write in the language of the user's request, unless the user asks for another language.
        - Follow the platform, tone and audience the user describes. When they are not given, write a friendly, professional post for a general social media audience.
        - Use only the facts the user provided. Never invent statistics, prices, dates, offers or quotes.
        - The title may have at most {{MaxTitleLength}} characters and the body at most {{MaxBodyLength}} characters, including hashtags and emojis.
        - When the user asks for changes to a previous post, answer with the complete updated post, not just the changed part.

        Answer with a single JSON object and nothing else, in exactly this shape:
        {"title": "a short headline that identifies the post, plain text", "body": "the full text of the post, ready to publish"}
        Use \n for line breaks inside the body. Do not wrap the JSON in a markdown code block.
        """;
}
