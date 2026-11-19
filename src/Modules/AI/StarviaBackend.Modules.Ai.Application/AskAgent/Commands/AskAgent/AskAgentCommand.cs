using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Ai.Application.AskAgent.Commands.AskAgent;

internal sealed record AskAgentCommand(AskAgentRequest Request, Guid UserId) : ICommand<AskAgentResult>;

/// <summary>
/// Prompt the social-media helper. Omit <see cref="ChatId"/> to start a new AskAgent chat;
/// send the returned chat id on the next call to continue the conversation.
/// </summary>
internal sealed record AskAgentRequest(
    string Prompt,
    Guid? ChatId = null,
    string? Title = null,
    Guid? BrandProfileId = null)
{
    public const int MaxPromptLength = 8000;
}

internal sealed record AskAgentResult(
    Guid ChatId,
    Guid MessageId,
    string Answer,
    string? Model,
    int? InputTokens,
    int? OutputTokens);
