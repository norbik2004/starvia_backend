using Microsoft.Extensions.AI;
using StarviaBackend.Modules.Ai.Application.Chats;
using StarviaBackend.Modules.Ai.Application.Chats.Access;
using StarviaBackend.Modules.Ai.Core.Chats.Access;
using StarviaBackend.Modules.Ai.Core.Chats.Entities;
using StarviaBackend.Modules.Ai.Core.Chats.Enums;
using StarviaBackend.Modules.Ai.Core.Chats.Exceptions;
using StarviaBackend.Modules.Ai.Core.Chats.Repositories;
using StarviaBackend.Modules.Brands.Integrations;
using StarviaBackend.Shared.Abstractions.Commands;
using StarviaBackend.Shared.Abstractions.Time;
using ModelMessage = Microsoft.Extensions.AI.ChatMessage;
using StoredMessage = StarviaBackend.Modules.Ai.Core.Chats.Entities.ChatMessage;

namespace StarviaBackend.Modules.Ai.Application.AskAgent.Commands.AskAgent;

internal sealed class AskAgentHandler(
    IChatRepository chatRepository,
    IChatAccessService access,
    IBrandsClient brandsClient,
    IChatClient chatClient,
    IClock clock)
    : ICommandHandler<AskAgentCommand, AskAgentResult>
{
    public async Task<AskAgentResult> HandleAsync(
        AskAgentCommand command,
        CancellationToken cancellationToken = default)
    {
        var request = command.Request;

        string? brandContext = null;
        if (request.BrandProfileId is { } brandProfileId)
        {
            var profile = await brandsClient.GetProfileAsync(command.UserId, brandProfileId, cancellationToken);
            brandContext = AskAgentPrompt.BrandContextOf(profile);
        }

        Chat chat;
        var isNewChat = request.ChatId is null;

        if (request.ChatId is { } chatId)
        {
            chat = await chatRepository.GetAllowedAsync(access, ChatAction.AddMessage, chatId, cancellationToken);

            if (chat.Type != ChatType.AskAgent)
            {
                throw new ChatTypeNotSupportedException(chat.Id, chat.Type, ChatType.AskAgent);
            }
        }
        else
        {
            var title = string.IsNullOrWhiteSpace(request.Title)
                ? AskAgentPrompt.DefaultChatTitle
                : request.Title.Trim();

            chat = Chat.Create(command.UserId, ChatType.AskAgent, title);
        }

        if (chat.Messages.Count == 0)
        {
            chat.AddSystemMessage(AskAgentPrompt.Instructions, clock.UtcNow);
        }

        chat.AddUserMessage(request.Prompt, clock.UtcNow);

        List<ModelMessage> messages =
        [
            new ModelMessage(ChatRole.System, AskAgentPrompt.Instructions),
        ];

        if (brandContext is not null)
        {
            messages.Add(new ModelMessage(ChatRole.System, brandContext));
        }

        foreach (var stored in chat.Messages.OrderBy(m => m.Sequence))
        {
            if (stored.Role is ChatMessageRole.System)
            {
                continue;
            }

            messages.Add(new ModelMessage(ToChatRole(stored.Role), stored.Content));
        }

        var response = await chatClient.GetResponseAsync(
            messages,
            new ChatOptions { MaxOutputTokens = AskAgentPrompt.MaxTokens },
            cancellationToken);

        var content = response.Text;
        var model = response.ModelId;
        var inputTokens = ToTokenCount(response.Usage?.InputTokenCount);
        var outputTokens = ToTokenCount(response.Usage?.OutputTokenCount);

        var answer = chat.AddAssistantMessage(
            content,
            clock.UtcNow,
            model is null ? null : FitModelName(model),
            inputTokens,
            outputTokens);

        if (isNewChat)
        {
            await chatRepository.AddAsync(chat, cancellationToken);
        }
        else
        {
            await chatRepository.UpdateAsync(chat, cancellationToken);
        }

        return new AskAgentResult(
            chat.Id,
            answer.Id,
            content,
            model,
            inputTokens,
            outputTokens);
    }

    private static ChatRole ToChatRole(ChatMessageRole role) => role switch
    {
        ChatMessageRole.User => ChatRole.User,
        ChatMessageRole.Assistant => ChatRole.Assistant,
        ChatMessageRole.System => ChatRole.System,
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown chat message role."),
    };

    private static int? ToTokenCount(long? count) => count is { } value ? (int)Math.Min(value, int.MaxValue) : null;

    private static string FitModelName(string model) =>
        model.Length <= StoredMessage.MaxModelLength ? model : model[..StoredMessage.MaxModelLength];
}
