using StarviaBackend.Modules.Emails.Application.Emails.Tokens;
using StarviaBackend.Shared.Abstractions.App;
using StarviaBackend.Shared.Abstractions.Commands;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Emails.Tests.Integration.Support;

internal sealed class StubAppUrls(string frontendBaseUrl = "https://app.example.com") : IAppUrls
{
    public string ApiBaseUrl => "https://api.example.com";

    public string FrontendBaseUrl { get; } = frontendBaseUrl;
}

internal sealed class StubAccountTokenProvider : IAccountTokenProvider
{
    public string ConfirmationToken { get; set; } = "confirmation-token";

    public string PasswordResetToken { get; set; } = "reset-token";

    /// <summary>When set, both methods throw it (simulates Accounts refusing to issue a token).</summary>
    public Exception? Failure { get; set; }

    public Task<string> GetEmailConfirmationTokenAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Failure is null ? Task.FromResult(ConfirmationToken) : Task.FromException<string>(Failure);

    public Task<string> GetPasswordResetTokenAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Failure is null ? Task.FromResult(PasswordResetToken) : Task.FromException<string>(Failure);
}

/// <summary>Records what is dispatched and answers with canned results (or fails on demand).</summary>
internal sealed class RecordingDispatcher : IDispatcher
{
    private readonly List<object> _commands = [];
    private readonly List<object> _queries = [];

    public IReadOnlyList<object> Commands
    {
        get { lock (_commands) { return [.. _commands]; } }
    }

    public IReadOnlyList<object> Queries
    {
        get { lock (_queries) { return [.. _queries]; } }
    }

    /// <summary>Thrown by every <c>SendAsync</c> call when set.</summary>
    public Exception? CommandFailure { get; set; }

    /// <summary>Produces the result of a command carrying a result; receives the command.</summary>
    public Func<object, object>? CommandResult { get; set; }

    /// <summary>Produces the result of a query; receives the query.</summary>
    public Func<object, object>? QueryResult { get; set; }

    public Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : class, ICommand
    {
        Record(_commands, command);
        return CommandFailure is null ? Task.CompletedTask : Task.FromException(CommandFailure);
    }

    public Task<TResult> SendAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : class, ICommand<TResult>
    {
        Record(_commands, command);

        if (CommandFailure is not null)
        {
            return Task.FromException<TResult>(CommandFailure);
        }

        return Task.FromResult(CommandResult is null ? default! : (TResult)CommandResult(command));
    }

    public Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        Record(_queries, query);

        return Task.FromResult(QueryResult is null ? default! : (TResult)QueryResult(query));
    }

    private static void Record(List<object> target, object item)
    {
        lock (target)
        {
            target.Add(item);
        }
    }
}
