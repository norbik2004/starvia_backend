using StarviaBackend.Shared.Abstractions.Commands;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Shared.Abstractions.Dispatchers;

/// <summary>
/// Single facade that endpoints inject. Routes commands/queries to their handlers
/// via <see cref="ICommandDispatcher"/> / <see cref="IQueryDispatcher"/>.
/// </summary>
public interface IDispatcher
{
    Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : class, ICommand;

    Task<TResult> SendAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : class, ICommand<TResult>;

    Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
}
