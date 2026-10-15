using MassTransit;
using Microsoft.Extensions.Logging;
using StarviaBackend.Modules.Accounts.Application.Users.Commands.GeneratePasswordResetToken;
using StarviaBackend.Shared.Abstractions.Accounts;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Exceptions;

namespace StarviaBackend.Modules.Accounts.Application.Users.Requests.GeneratePasswordResetToken;

/// <summary>
/// Answers the Emails module's request for a password reset token. Expected business failures
/// are returned as <see cref="UserTokenRejected"/> so the requester doesn't retry them;
/// anything else faults the request.
/// </summary>
internal sealed class GeneratePasswordResetTokenConsumer(
    IDispatcher dispatcher,
    ILogger<GeneratePasswordResetTokenConsumer> logger)
    : IConsumer<GeneratePasswordResetTokenRequest>
{
    public async Task Consume(ConsumeContext<GeneratePasswordResetTokenRequest> context)
    {
        var userId = context.Message.UserId;

        try
        {
            var result = await dispatcher.SendAsync<GeneratePasswordResetTokenCommand, GeneratePasswordResetTokenResult>(
                new GeneratePasswordResetTokenCommand(userId),
                context.CancellationToken);

            await context.RespondAsync(new UserTokenGenerated(result.Token));
        }
        catch (BusinessException ex)
        {
            logger.LogWarning(
                "Rejected password reset token request for user {UserId}: {Code}",
                userId,
                ex.Code);

            await context.RespondAsync(new UserTokenRejected(ex.Code, ex.Message));
        }
    }
}
