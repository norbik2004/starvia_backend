using Microsoft.Extensions.AI;

namespace StarviaBackend.Modules.Ai.Tests.Unit.Fakes;

/// <summary>One call to the model: the conversation that was sent and the options it was sent with.</summary>
internal sealed record ModelRequest(IReadOnlyList<ChatMessage> Messages, ChatOptions? Options);

/// <summary>Stands in for the model: remembers what it was asked and answers with a canned reply.</summary>
internal sealed class FakeChatClient : IChatClient
{
    public const string DefaultTitle = "Fresh coffee";
    public const string DefaultBody = "Try our new blend.\nAvailable today.";

    public List<ModelRequest> Requests { get; } = [];

    public string Reply { get; set; } = PostJson(DefaultTitle, DefaultBody);
    public string? Model { get; set; } = "fake/model-1";
    public int? InputTokens { get; set; } = 42;
    public int? OutputTokens { get; set; } = 17;

    /// <summary>When set, the call fails with it, like the real client does when Google AI Studio is down.</summary>
    public Exception? Failure { get; set; }

    public static string PostJson(string title, string body) =>
        System.Text.Json.JsonSerializer.Serialize(new { title, body });

    public Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        Requests.Add(new ModelRequest(messages.ToList(), options));

        if (Failure is not null)
        {
            throw Failure;
        }

        var response = new ChatResponse(new ChatMessage(ChatRole.Assistant, Reply))
        {
            ModelId = Model,
            Usage = InputTokens is null && OutputTokens is null
                ? null
                : new UsageDetails { InputTokenCount = InputTokens, OutputTokenCount = OutputTokens },
        };

        return Task.FromResult(response);
    }

    public IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default) =>
        throw new NotSupportedException("The module never streams.");

    public object? GetService(Type serviceType, object? serviceKey = null) => null;

    public void Dispose()
    {
    }
}
