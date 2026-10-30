using Microsoft.Extensions.AI;

namespace StarviaBackend.Modules.Ai.Tests.Integration;

/// <summary>One call to the model: the conversation that was sent and the options it was sent with.</summary>
public sealed record ModelRequest(IReadOnlyList<ChatMessage> Messages, ChatOptions? Options);

/// <summary>
/// Replaces the Google AI Studio-backed chat client so the AI tests never reach the internet or use up the quota.
/// By default it answers with a well-formed post.
/// </summary>
public sealed class FakeChatClient : IChatClient
{
    public const string DefaultTitle = "Fresh coffee";
    public const string DefaultBody = "Try our new blend.\nAvailable today.";
    public const string DefaultModel = "fake/model-1";

    /// <summary>Everything the application asked the model, oldest request first.</summary>
    public List<ModelRequest> Requests { get; } = [];

    /// <summary>The raw text the "model" answers with.</summary>
    public string Reply { get; set; } = PostJson(DefaultTitle, DefaultBody);

    /// <summary>When set, every call fails with it, like the real client does when Google AI Studio is down.</summary>
    public Exception? Failure { get; set; }

    public void Reset()
    {
        Requests.Clear();
        Reply = PostJson(DefaultTitle, DefaultBody);
        Failure = null;
    }

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
            ModelId = DefaultModel,
            Usage = new UsageDetails { InputTokenCount = 42, OutputTokenCount = 17 },
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
