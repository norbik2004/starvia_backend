using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Ai.Application.PostGeneration.Commands.GeneratePost;

internal sealed record GeneratePostCommand(GeneratePostRequest Request, Guid UserId)
    : ICommand<GeneratePostResult>;

/// <summary>
/// What the user wants to be written. The platform, tone, language and audience are part of the prompt itself
/// ("a LinkedIn post about..."), so the same endpoint serves a first draft and every later refinement.
/// </summary>
/// <param name="Prompt">The request, kept in the chat exactly as typed.</param>
/// <param name="ChatId">
/// A post generation chat to continue, e.g. to ask for changes to the previous draft.
/// When empty, a new chat is started for this prompt.
/// </param>
internal sealed record GeneratePostRequest(string Prompt, Guid? ChatId = null)
{
    public const int MaxPromptLength = 4000;
}

/// <summary>The generated post together with the chat that now holds the conversation.</summary>
/// <param name="ChatId">The chat to send the next prompt to when the post needs changes.</param>
/// <param name="MessageId">The chat message that stores the raw answer of the AI.</param>
/// <param name="Title">Short headline of the post.</param>
/// <param name="Body">Full text of the post, ready to be saved as the body of a post.</param>
/// <param name="Model">The AI model that wrote the post, when the provider reported it.</param>
/// <param name="InputTokens">Tokens of the prompt, when the provider reported them.</param>
/// <param name="OutputTokens">Tokens of the answer, when the provider reported them.</param>
internal sealed record GeneratePostResult(
    Guid ChatId,
    Guid MessageId,
    string Title,
    string Body,
    string? Model,
    int? InputTokens,
    int? OutputTokens);
