namespace StarviaBackend.Modules.Ai.Core.Chats.Enums;

public enum ChatMessageRole
{
    /// <summary>Instructions given to the model (not typed by the user).</summary>
    System = 1,

    /// <summary>A prompt written by the user.</summary>
    User = 2,

    /// <summary>An answer produced by the AI.</summary>
    Assistant = 3
}
