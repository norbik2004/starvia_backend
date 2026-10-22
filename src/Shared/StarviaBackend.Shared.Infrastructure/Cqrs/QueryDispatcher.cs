using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Shared.Infrastructure.Cqrs;

internal sealed class QueryDispatcher(IServiceProvider serviceProvider) : IQueryDispatcher
{
    public async Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
    {
        // The concrete query type is only known at runtime, so resolve the closed handler reflectively.
        // We invoke through the *interface* method (public) rather than `dynamic`, because handlers are
        // internal and dynamic binding cannot reach internal members across assemblies.
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
        var handler = serviceProvider.GetRequiredService(handlerType);
        var method = handlerType.GetMethod("HandleAsync")!;

        var task = (Task<TResult>)method.Invoke(handler, [query, cancellationToken])!;
        return await task;
    }
}
