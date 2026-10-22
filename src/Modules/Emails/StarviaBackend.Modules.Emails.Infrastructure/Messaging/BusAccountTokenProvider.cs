using MassTransit;
using StarviaBackend.Modules.Emails.Application.Emails.Tokens;
using StarviaBackend.Modules.Emails.Core.Emails.Exceptions;
using StarviaBackend.Shared.Abstractions.Accounts;

namespace StarviaBackend.Modules.Emails.Infrastructure.Messaging;

/// <summary>
/// Gets account tokens from the Accounts module through MassTransit request/response over the
/// bus (RabbitMQ). Emails only knows the contracts in <c>Shared.Abstractions.Accounts</c>, never
/// the Accounts module itself.
/// </summary>
/// <remarks>
/// A business refusal comes back as <see cref="UserTokenRejected"/> and is surfaced as
/// <see cref="AccountTokenRejectedException"/>. Transport problems (timeout, faulted responder)
/// propagate as MassTransit exceptions so the consumer's retry policy can handle them.
/// </remarks>
internal sealed class BusAccountTokenProvider(
    IRequestClient<GenerateEmailConfirmationTokenRequest> emailConfirmationClient,
    IRequestClient<GeneratePasswordResetTokenRequest> passwordResetClient)
    : IAccountTokenProvider
{
    public Task<string> GetEmailConfirmationTokenAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
        => RequestTokenAsync(
            emailConfirmationClient,
            new GenerateEmailConfirmationTokenRequest(userId),
            userId,
            cancellationToken);

    public Task<string> GetPasswordResetTokenAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
        => RequestTokenAsync(
            passwordResetClient,
            new GeneratePasswordResetTokenRequest(userId),
            userId,
            cancellationToken);

    private static async Task<string> RequestTokenAsync<TRequest>(
        IRequestClient<TRequest> client,
        TRequest request,
        Guid userId,
        CancellationToken cancellationToken)
        where TRequest : class
    {
        var response = await client.GetResponse<UserTokenGenerated, UserTokenRejected>(
            request,
            cancellationToken);

        if (response.Is(out Response<UserTokenGenerated>? generated))
        {
            return generated.Message.Token;
        }

        if (response.Is(out Response<UserTokenRejected>? rejected))
        {
            throw new AccountTokenRejectedException(userId, rejected.Message.Code, rejected.Message.Reason);
        }

        throw new InvalidOperationException(
            $"Unexpected response to {typeof(TRequest).Name}: {response.Message?.GetType().Name ?? "null"}.");
    }
}
