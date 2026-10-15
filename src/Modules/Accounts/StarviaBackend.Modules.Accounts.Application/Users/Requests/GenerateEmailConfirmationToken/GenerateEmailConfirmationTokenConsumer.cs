using MassTransit;
using Microsoft.Extensions.Logging;
using StarviaBackend.Modules.Accounts.Application.Users.Commands.GenerateEmailConfirmationToken;
using StarviaBackend.Shared.Abstractions.Accounts;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Exceptions;

namespace StarviaBackend.Modules.Accounts.Application.Users.Requests.GenerateEmailConfirmationToken;

/// <summary>
/// Answers the Emails module's request for an e-mail confirmation token. Expected business
/// failures are returned as <see cref="UserTokenRejected"/> so the requester doesn't retry them;
/// anything else faults the request.
/// </summary>
internal sealed class GenerateEmailConfirmationTokenConsumer(
    IDispatcher dispatcher,
    ILogger<GenerateEmailConfirmationTokenConsumer> logger)
    : IConsumer<GenerateEmailConfirmationTokenRequest>
{
    public async Task Consume(ConsumeContext<GenerateEmailConfirmationTokenRequest> context)
    {
        var userId = context.Message.UserId;

        try
        {
            var result = await dispatcher.SendAsync<GenerateEmailConfirmationTokenCommand, GenerateEmailConfirmationTokenResult>(
                new GenerateEmailConfirmationTokenCommand(userId),
                context.CancellationToken);

            await context.RespondAsync(new UserTokenGenerated(result.Token));
        }
        catch (BusinessException ex)
        {
            logger.LogWarning(
                "Rejected e-mail confirmation token request for user {UserId}: {Code}",
                userId,
                ex.Code);

            await context.RespondAsync(new UserTokenRejected(ex.Code, ex.Message));
        }
    }
}
