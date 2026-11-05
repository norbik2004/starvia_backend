using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Ai.Application.PostGeneration.Commands.GeneratePost;

internal sealed record GeneratePostCommand(GeneratePostRequest Request, Guid UserId)
    : ICommand<GeneratePostResult>;

internal sealed record GeneratePostRequest(string Prompt, Guid PostId)
{
    public const int MaxPromptLength = 4000;
}

internal sealed record GeneratePostResult(
    Guid ChatId,
    Guid MessageId,
    string Body,
    string? Model,
    int? InputTokens,
    int? OutputTokens);
