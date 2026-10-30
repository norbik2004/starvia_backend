namespace StarviaBackend.Modules.Ai.Core.Chats.Enums;

/// <summary>
/// Kind of AI agent a <see cref="Entities.Chat"/> talks to. It decides which system prompt,
/// tools and rules the application layer applies to the conversation.
/// </summary>
/// <remarks>
/// Persisted as an integer. Values are explicit and must never be renumbered or reused;
/// add new chat kinds at the end.
/// </remarks>
public enum ChatType
{
    /// <summary>General helper ("ask agent") that answers the user's questions.</summary>
    AskAgent = 1,

    /// <summary>Agent that generates and refines the content of posts.</summary>
    PostContentGeneration = 2
}
