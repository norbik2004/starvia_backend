using Microsoft.Extensions.AI;
using StarviaBackend.Modules.Ai.Application.Chats;
using StarviaBackend.Modules.Ai.Core.Chats.Entities;
using StarviaBackend.Modules.Ai.Core.Chats.Enums;
using StarviaBackend.Modules.Ai.Core.Chats.Exceptions;
using StarviaBackend.Modules.Ai.Core.Chats.Repositories;
using StarviaBackend.Modules.Brands.Integrations;
using StarviaBackend.Modules.Brands.Integrations.Models;
using StarviaBackend.Modules.Posts.Integrations;
using StarviaBackend.Shared.Abstractions.Commands;
using StarviaBackend.Shared.Abstractions.Time;
using ModelMessage = Microsoft.Extensions.AI.ChatMessage;
using StoredMessage = StarviaBackend.Modules.Ai.Core.Chats.Entities.ChatMessage;

namespace StarviaBackend.Modules.Ai.Application.PostGeneration.Commands.GeneratePost;

internal sealed class GeneratePostHandler(
    IChatRepository chatRepository,
    IPostsClient postsClient,
    IBrandsClient brandsClient,
    IChatClient chatClient,
    IClock clock)
    : ICommandHandler<GeneratePostCommand, GeneratePostResult>
{
    public async Task<GeneratePostResult> HandleAsync(
        GeneratePostCommand command,
        CancellationToken cancellationToken = default)
    {
        var request = command.Request;

        var posts = await postsClient.GetPostsAsync(command.UserId, [request.PostId], cancellationToken);
        var post = posts.Single();

        string? brandContext = null;
        if (request.BrandProfileId is { } brandProfileId)
        {
            var profile = await brandsClient.GetProfileAsync(command.UserId, brandProfileId, cancellationToken);
            var generation = MergeGeneration(profile.GenerationDefaults, request.GenerationOverrides);
            brandContext = PostGenerationPrompt.BrandContextOf(profile, generation);
        }

        var chat = await chatRepository.GetByPostIdAsync(request.PostId, cancellationToken);
        var isNewChat = chat is null;

        if (chat is null)
        {
            chat = Chat.Create(command.UserId, ChatType.PostContentGeneration, $"Generowanie postu: {post.Title}");
            chat.LinkPost(request.PostId);
        }

        if (chat.Type != ChatType.PostContentGeneration)
        {
            throw new ChatTypeNotSupportedException(chat.Id, chat.Type, ChatType.PostContentGeneration);
        }

        if (chat.Messages.Count == 0)
        {
            chat.AddSystemMessage(PostGenerationPrompt.Instructions, clock.UtcNow);
        }

        chat.AddUserMessage(request.Prompt, clock.UtcNow);

        List<ModelMessage> messages =
        [
            new ModelMessage(ChatRole.System, PostGenerationPrompt.Instructions),
            new ModelMessage(ChatRole.System, PostGenerationPrompt.PlatformContextOf(post.Platforms)),
            new ModelMessage(ChatRole.System, PostGenerationPrompt.CurrentContentOf(post.Body)),
        ];

        if (brandContext is not null)
        {
            messages.Add(new ModelMessage(ChatRole.System, brandContext));
        }

        messages.Add(new ModelMessage(ChatRole.User, request.Prompt));

        var response = await chatClient.GetResponseAsync(
            messages,
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

        var generated = GeneratedPostParser.Parse(content);

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
            generated.Body,
            model,
            inputTokens,
            outputTokens);
    }

    private static BrandGenerationDefaults MergeGeneration(
        BrandGenerationDefaults? defaults,
        BrandGenerationDefaults? overrides) =>
        new(
            overrides?.PostType ?? defaults?.PostType,
            overrides?.Goal ?? defaults?.Goal,
            overrides?.Language ?? defaults?.Language);

    private static int? ToTokenCount(long? count) => count is { } value ? (int)Math.Min(value, int.MaxValue) : null;

    private static string FitModelName(string model) =>
        model.Length <= StoredMessage.MaxModelLength ? model : model[..StoredMessage.MaxModelLength];
}
