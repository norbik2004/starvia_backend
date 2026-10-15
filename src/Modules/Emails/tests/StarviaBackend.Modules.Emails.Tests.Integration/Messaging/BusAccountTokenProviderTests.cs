using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Emails.Application.Emails.Tokens;
using StarviaBackend.Modules.Emails.Core.Emails.Exceptions;
using StarviaBackend.Modules.Emails.Infrastructure.Messaging;
using StarviaBackend.Shared.Abstractions.Accounts;

namespace StarviaBackend.Modules.Emails.Tests.Integration.Messaging;

/// <summary>
/// Emails must not reference Accounts, so it fetches account tokens over the bus. These tests run
/// the real <see cref="BusAccountTokenProvider"/> against stand-in responders for the
/// <c>Shared.Abstractions.Accounts</c> contracts (in-memory harness: no RabbitMQ, no DB).
/// </summary>
public sealed class BusAccountTokenProviderTests
{
    [Fact]
    public async Task Email_confirmation_token_comes_from_the_confirmation_responder()
    {
        var userId = Guid.NewGuid();
        await using var provider = BuildProvider(new StubAccounts());
        await StartAsync(provider);

        var token = await Resolve(provider).GetEmailConfirmationTokenAsync(userId);

        token.Should().Be($"confirm:{userId}");
    }

    [Fact]
    public async Task Password_reset_token_comes_from_the_reset_responder()
    {
        var userId = Guid.NewGuid();
        await using var provider = BuildProvider(new StubAccounts());
        await StartAsync(provider);

        var token = await Resolve(provider).GetPasswordResetTokenAsync(userId);

        token.Should().Be($"reset:{userId}");
    }

    [Fact]
    public async Task Rejection_is_surfaced_as_AccountTokenRejectedException()
    {
        var userId = Guid.NewGuid();
        var accounts = new StubAccounts { Rejection = new UserTokenRejected("email_arleady_confirmed", "already done") };
        await using var provider = BuildProvider(accounts);
        await StartAsync(provider);

        var act = () => Resolve(provider).GetEmailConfirmationTokenAsync(userId);

        var exception = (await act.Should().ThrowAsync<AccountTokenRejectedException>()).Which;
        exception.RejectionCode.Should().Be("email_arleady_confirmed");
        exception.Message.Should().Contain(userId.ToString());
    }

    [Fact]
    public async Task Unanswered_request_fails_instead_of_returning_a_token()
    {
        // No responders registered: stands in for Accounts being down. The failure must propagate
        // (so the consumer's retry policy kicks in), not be swallowed or mapped to a rejection.
        await using var provider = new ServiceCollection()
            .AddLogging()
            .AddMassTransitTestHarness(x =>
            {
                x.AddRequestClient<GenerateEmailConfirmationTokenRequest>(RequestTimeout.After(s: 1));
                x.AddRequestClient<GeneratePasswordResetTokenRequest>(RequestTimeout.After(s: 1));
            })
            .AddScoped<IAccountTokenProvider, BusAccountTokenProvider>()
            .BuildServiceProvider(true);
        await StartAsync(provider);

        var act = () => Resolve(provider).GetEmailConfirmationTokenAsync(Guid.NewGuid());

        await act.Should().ThrowAsync<RequestTimeoutException>();
    }

    private static ServiceProvider BuildProvider(StubAccounts accounts) =>
        new ServiceCollection()
            .AddLogging()
            .AddSingleton(accounts)
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<ConfirmationResponder>();
                x.AddConsumer<ResetResponder>();
                x.AddRequestClient<GenerateEmailConfirmationTokenRequest>(RequestTimeout.After(s: 10));
                x.AddRequestClient<GeneratePasswordResetTokenRequest>(RequestTimeout.After(s: 10));
            })
            .AddScoped<IAccountTokenProvider, BusAccountTokenProvider>()
            .BuildServiceProvider(true);

    private static async Task StartAsync(ServiceProvider provider) =>
        await provider.GetRequiredService<ITestHarness>().Start();

    private static IAccountTokenProvider Resolve(ServiceProvider provider) =>
        provider.CreateScope().ServiceProvider.GetRequiredService<IAccountTokenProvider>();

    private sealed class StubAccounts
    {
        public UserTokenRejected? Rejection { get; init; }
    }

    private sealed class ConfirmationResponder(StubAccounts accounts)
        : IConsumer<GenerateEmailConfirmationTokenRequest>
    {
        public Task Consume(ConsumeContext<GenerateEmailConfirmationTokenRequest> context) =>
            accounts.Rejection is { } rejection
                ? context.RespondAsync(rejection)
                : context.RespondAsync(new UserTokenGenerated($"confirm:{context.Message.UserId}"));
    }

    private sealed class ResetResponder(StubAccounts accounts)
        : IConsumer<GeneratePasswordResetTokenRequest>
    {
        public Task Consume(ConsumeContext<GeneratePasswordResetTokenRequest> context) =>
            accounts.Rejection is { } rejection
                ? context.RespondAsync(rejection)
                : context.RespondAsync(new UserTokenGenerated($"reset:{context.Message.UserId}"));
    }
}
