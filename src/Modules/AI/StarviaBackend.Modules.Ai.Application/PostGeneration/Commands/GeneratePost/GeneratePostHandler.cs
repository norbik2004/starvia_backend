using Microsoft.Extensions.AI;
using StarviaBackend.Modules.Ai.Application.Chats;
using StarviaBackend.Modules.Ai.Core.Chats.Entities;
using StarviaBackend.Modules.Ai.Core.Chats.Enums;
using StarviaBackend.Modules.Ai.Core.Chats.Exceptions;
using StarviaBackend.Modules.Ai.Core.Chats.Repositories;
using StarviaBackend.Shared.Abstractions.Commands;
using StarviaBackend.Shared.Abstractions.Time;
using ModelMessage = Microsoft.Extensions.AI.ChatMessage;
using StoredMessage = StarviaBackend.Modules.Ai.Core.Chats.Entities.ChatMessage;

namespace StarviaBackend.Modules.Ai.Application.PostGeneration.Commands.GeneratePost;

/// <summary>
/// Writes a post with the AI. The conversation lives in a <see cref="ChatType.PostContentGeneration"/> chat: the
/// prompt and the answer are stored in it, so the next call with the same chat id refines the post instead of
/// starting over. Nothing is stored unless the AI produced a usable post, which keeps failed attempts out of the
/// history and makes a retry safe. The model is reached through <see cref="IChatClient"/>, the vendor-neutral
/// interface of Microsoft.Extensions.AI: which provider answers is decided by the registration in Infrastructure.
/// </summary>
internal sealed class GeneratePostHandler(
    IChatRepository chatRepository,
    IChatClient chatClient,
    IClock clock)
    : ICommandHandler<GeneratePostCommand, GeneratePostResult>
{
    public async Task<GeneratePostResult> HandleAsync(
        GeneratePostCommand command,
        CancellationToken cancellationToken = default)
    {
        var request = command.Request;
        var isNewChat = request.ChatId is null;

        var chat = request.ChatId is { } chatId
            ? await chatRepository.GetOwnedAsync(chatId, command.UserId, cancellationToken)
            : Chat.Create(command.UserId, ChatType.PostContentGeneration, TitleFrom(request.Prompt));

        if (chat.Type != ChatType.PostContentGeneration)
        {
            throw new ChatTypeNotSupportedException(chat.Id, chat.Type, ChatType.PostContentGeneration);
        }

        // Added to the chat in memory only: it is persisted together with the answer or not at all.
        if (chat.Messages.Count == 0)
        {
            chat.AddSystemMessage(PostGenerationPrompt.Instructions, clock.UtcNow);
        }

        chat.AddUserMessage(request.Prompt, clock.UtcNow);

        // No model is named here: the one configured for the client answers. No sampling parameters either: the
        // model's defaults are the ones its vendor tunes it for.
        var response = await chatClient.GetResponseAsync(
            ConversationOf(chat),
            new ChatOptions
            {
                ResponseFormat = ChatResponseFormat.Json,
                MaxOutputTokens = PostGenerationPrompt.MaxTokens,
            },
            cancellationToken);

        var content = response.Text;
        var model = response.ModelId;
        var inputTokens = ToTokenCount(response.Usage?.InputTokenCount);
        var outputTokens = ToTokenCount(response.Usage?.OutputTokenCount);

        var post = GeneratedPostParser.Parse(content);

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

        return new GeneratePostResult(
            chat.Id,
            answer.Id,
            post.Title,
            post.Body,
            model,
            inputTokens,
            outputTokens);
    }

    /// <summary>
    /// The stored history, replayed to the model as-is. A chat whose first messages were typed in through the
    /// plain "add message" endpoint has no stored instructions, so they are put in front for this call.
    /// </summary>
    private static List<ModelMessage> ConversationOf(Chat chat)
    {
        var messages = chat.Messages
            .OrderBy(message => message.Sequence)
            .Select(message => new ModelMessage(ToModelRole(message.Role), message.Content))
            .ToList();

        if (messages.TrueForAll(message => message.Role != ChatRole.System))
        {
            messages.Insert(0, new ModelMessage(ChatRole.System, PostGenerationPrompt.Instructions));
        }

        return messages;
    }

    private static ChatRole ToModelRole(ChatMessageRole role) => role switch
    {
        ChatMessageRole.System => ChatRole.System,
        ChatMessageRole.User => ChatRole.User,
        ChatMessageRole.Assistant => ChatRole.Assistant,
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Unknown chat message role."),
    };

    /// <summary>The library counts tokens as <see cref="long"/>; a single answer never comes near the int range.</summary>
    private static int? ToTokenCount(long? count) => count is { } value ? (int)Math.Min(value, int.MaxValue) : null;

    /// <summary>The chat is named after the prompt: one line, shortened to fit the title of a chat.</summary>
    private static string TitleFrom(string prompt)
    {
        var singleLine = string.Join(' ', prompt.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));

        if (singleLine.Length <= Chat.MaxTitleLength)
        {
            return singleLine;
        }

        var length = Chat.MaxTitleLength - 1;

        // Never cut an emoji (a surrogate pair) in half: the lone half is not valid text.
        if (char.IsHighSurrogate(singleLine[length - 1]))
        {
            length--;
        }

        return singleLine[..length].TrimEnd() + "…";
    }

    /// <summary>Providers can report long versioned model names. A name that does not fit must not lose the answer.</summary>
    private static string FitModelName(string model) =>
        model.Length <= StoredMessage.MaxModelLength ? model : model[..StoredMessage.MaxModelLength];
}
