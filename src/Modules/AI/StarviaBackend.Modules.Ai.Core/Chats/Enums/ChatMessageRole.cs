namespace StarviaBackend.Modules.Ai.Core.Chats.Enums;

/// <summary>
/// Author of a <see cref="Entities.ChatMessage"/>. Mirrors the roles used by chat-completion APIs
/// so the stored history can be replayed to the model as-is.
/// </summary>
/// <remarks>Persisted as an integer. Never renumber or reuse values.</remarks>
public enum ChatMessageRole
{
    /// <summary>Instructions given to the model (not typed by the user).</summary>
    System = 1,

    /// <summary>A prompt written by the user.</summary>
    User = 2,

    /// <summary>An answer produced by the AI.</summary>
    Assistant = 3
}
