using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using StarviaBackend.Modules.Ai.Core.Chats.Enums;
using StarviaBackend.Shared.Abstractions.Auth;
using StarviaBackend.Shared.Abstractions.Exceptions;
using StarviaBackend.Shared.Tests.Integration;

namespace StarviaBackend.Modules.Ai.Tests.Integration;

/// <summary>
/// All AI integration tests share one container (per collection) and run sequentially: the
/// Testcontainers connection string is passed via a process-wide env var, so parallel classes
/// would race. New test classes just derive this and belong to the same collection.
/// </summary>
[Collection(AiCollection.Name)]
public abstract class AiIntegrationTest : IntegrationTest<AiApp>
{
    protected const string ChatsUrl = "/v1/ai/chats";
    protected const string GeneratePostUrl = "/v1/ai/posts/generate";

    protected AiIntegrationTest(AiApp app) : base(app)
    {
        app.TextModel.Reset();
        app.Posts.Reset();
        app.UseRealChatClient = false;
    }

    protected HttpClient CreateUserClient(Guid? userId = null) =>
        CreateClient(userId ?? Guid.NewGuid(), UserRoles.User);

    protected HttpClient CreateAdminClient(Guid? userId = null) =>
        CreateClient(userId ?? Guid.NewGuid(), UserRoles.Admin);

    protected static async Task<Guid> CreateChatAsync(
        HttpClient client,
        ChatType type = ChatType.AskAgent,
        string title = "My chat")
    {
        var response = await client.PostAsJsonAsync(ChatsUrl, new { type, title });
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<ChatIdResponse>();
        return body!.ChatId;
    }

    protected static Task<HttpResponseMessage> AddMessageAsync(HttpClient client, Guid chatId, string content) =>
        client.PostAsJsonAsync($"{ChatsUrl}/{chatId}/messages", new { content });

    protected static async Task<ChatResponse> GetChatAsync(HttpClient client, Guid chatId)
    {
        var response = await client.GetAsync($"{ChatsUrl}/{chatId}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        return (await response.Content.ReadFromJsonAsync<ChatResponse>())!;
    }

    protected static async Task AssertErrorCodeAsync(HttpResponseMessage response, HttpStatusCode status, string code)
    {
        response.StatusCode.Should().Be(status);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        error!.Errors.Should().Contain(e => e.Code == code);
    }

    protected sealed record ChatIdResponse(Guid ChatId);

    protected sealed record MessageIdResponse(Guid MessageId, int Sequence);

    protected sealed record ChatMessageResponse(
        Guid Id,
        int Sequence,
        ChatMessageRole Role,
        string Content,
        string? Model,
        int? InputTokens,
        int? OutputTokens,
        DateTime CreatedAt);

    protected sealed record ChatResponse(
        Guid Id,
        ChatType Type,
        string Title,
        ChatStatus Status,
        Guid? PostId,
        DateTime? LastMessageAt,
        DateTime CreatedAt,
        Guid OwnerId,
        List<ChatMessageResponse> Messages);

    protected sealed record ChatSummaryResponse(
        Guid Id,
        ChatType Type,
        string Title,
        ChatStatus Status,
        Guid? PostId,
        DateTime? LastMessageAt,
        DateTime CreatedAt,
        Guid OwnerId);

    protected sealed record ChatPageResponse(
        List<ChatSummaryResponse> Items,
        int Page,
        int PageSize,
        long TotalCount,
        int TotalPages);
}

[CollectionDefinition(AiCollection.Name)]
public sealed class AiCollection : ICollectionFixture<AiApp>
{
    public const string Name = "AiIntegration";
}
