namespace StarviaBackend.Modules.Ai.Core.Chats.Enums;

/// <remarks>Persisted as an integer. Never renumber or reuse values.</remarks>
public enum ChatStatus
{
    /// <summary>The conversation is open and accepts new messages.</summary>
    Active = 1,

    /// <summary>The conversation is kept in the history but is read-only.</summary>
    Archived = 2
}
