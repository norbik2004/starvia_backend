using StarviaBackend.Shared.Abstractions.Commands;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Shared.Infrastructure.Cqrs;

internal sealed class InMemoryDispatcher(
    ICommandDispatcher commandDispatcher,
    IQueryDispatcher queryDispatcher) : IDispatcher
{
    public Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : class, ICommand
        => commandDispatcher.SendAsync(command, cancellationToken);

    public Task<TResult> SendAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : class, ICommand<TResult>
        => commandDispatcher.SendAsync<TCommand, TResult>(command, cancellationToken);

    public Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
        => queryDispatcher.QueryAsync(query, cancellationToken);
}
