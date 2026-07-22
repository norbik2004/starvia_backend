using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Shared.Infrastructure.Cqrs;

internal sealed class CommandDispatcher(IServiceProvider serviceProvider) : ICommandDispatcher
{
    public async Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : class, ICommand
    {
        await ValidateAsync(command, cancellationToken);
        var handler = serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();
        await handler.HandleAsync(command, cancellationToken);
    }

    public async Task<TResult> SendAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
        where TCommand : class, ICommand<TResult>
    {
        await ValidateAsync(command, cancellationToken);
        var handler = serviceProvider.GetRequiredService<ICommandHandler<TCommand, TResult>>();
        return await handler.HandleAsync(command, cancellationToken);
    }

    private async Task ValidateAsync<TCommand>(TCommand command, CancellationToken cancellationToken)
        where TCommand : class
    {
        var validator = serviceProvider.GetService<IValidator<TCommand>>();
        if (validator is null)
        {
            return;
        }

        await validator.ValidateAndThrowAsync(command, cancellationToken);
    }
}
