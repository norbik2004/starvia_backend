using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Accounts.Application.Users.Commands.GenerateEmailConfirmationToken;
using StarviaBackend.Modules.Accounts.Application.Users.Commands.GeneratePasswordResetToken;
using StarviaBackend.Modules.Accounts.Application.Users.Requests.GenerateEmailConfirmationToken;
using StarviaBackend.Modules.Accounts.Application.Users.Requests.GeneratePasswordResetToken;
using StarviaBackend.Modules.Accounts.Core.Users.Exceptions;
using StarviaBackend.Shared.Abstractions.Accounts;
using StarviaBackend.Shared.Abstractions.Commands;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Accounts.Tests.Integration.Messaging;

/// <summary>
/// The Emails module asks Accounts for tokens over the bus (it must not reference Accounts).
/// These tests pin the Accounts side of that contract using the in-memory harness: no DB, no HTTP.
/// </summary>
public sealed class UserTokenRequestMessagingTests
{
    [Fact]
    public async Task Email_confirmation_request_is_answered_with_the_generated_token()
    {
        var userId = Guid.NewGuid();
        await using var provider = BuildProvider(command =>
            command is GenerateEmailConfirmationTokenCommand c && c.UserId == userId
                ? new GenerateEmailConfirmationTokenResult("confirm-token")
                : throw new InvalidOperationException("Unexpected command."));
        var harness = await StartAsync(provider);

        var response = await harness.GetRequestClient<GenerateEmailConfirmationTokenRequest>()
            .GetResponse<UserTokenGenerated, UserTokenRejected>(new GenerateEmailConfirmationTokenRequest(userId));

        response.Is(out Response<UserTokenGenerated>? generated).Should().BeTrue();
        generated!.Message.Token.Should().Be("confirm-token");
    }

    [Fact]
    public async Task Email_confirmation_request_for_confirmed_user_is_rejected_not_faulted()
    {
        await using var provider = BuildProvider(
            _ => throw new EmailArleadyConfirmedException("user@example.com"));
        var harness = await StartAsync(provider);

        var response = await harness.GetRequestClient<GenerateEmailConfirmationTokenRequest>()
            .GetResponse<UserTokenGenerated, UserTokenRejected>(
                new GenerateEmailConfirmationTokenRequest(Guid.NewGuid()));

        response.Is(out Response<UserTokenRejected>? rejected).Should().BeTrue();
        rejected!.Message.Code.Should().Be("email_arleady_confirmed");
    }

    [Fact]
    public async Task Password_reset_request_is_answered_with_the_generated_token()
    {
        var userId = Guid.NewGuid();
        await using var provider = BuildProvider(command =>
            command is GeneratePasswordResetTokenCommand c && c.UserId == userId
                ? new GeneratePasswordResetTokenResult("reset-token")
                : throw new InvalidOperationException("Unexpected command."));
        var harness = await StartAsync(provider);

        var response = await harness.GetRequestClient<GeneratePasswordResetTokenRequest>()
            .GetResponse<UserTokenGenerated, UserTokenRejected>(new GeneratePasswordResetTokenRequest(userId));

        response.Is(out Response<UserTokenGenerated>? generated).Should().BeTrue();
        generated!.Message.Token.Should().Be("reset-token");
    }

    [Fact]
    public async Task Password_reset_request_for_unknown_user_is_rejected_not_faulted()
    {
        var userId = Guid.NewGuid();
        await using var provider = BuildProvider(_ => throw new UserNotFoundException(userId));
        var harness = await StartAsync(provider);

        var response = await harness.GetRequestClient<GeneratePasswordResetTokenRequest>()
            .GetResponse<UserTokenGenerated, UserTokenRejected>(new GeneratePasswordResetTokenRequest(userId));

        response.Is(out Response<UserTokenRejected>? rejected).Should().BeTrue();
        rejected!.Message.Code.Should().Be("user_not_found");
    }

    [Fact]
    public async Task Unexpected_failure_faults_the_request_so_the_requester_can_retry()
    {
        await using var provider = BuildProvider(_ => throw new InvalidOperationException("db down"));
        var harness = await StartAsync(provider);

        var act = () => harness.GetRequestClient<GeneratePasswordResetTokenRequest>()
            .GetResponse<UserTokenGenerated, UserTokenRejected>(
                new GeneratePasswordResetTokenRequest(Guid.NewGuid()));

        await act.Should().ThrowAsync<RequestFaultException>();
    }

    private static ServiceProvider BuildProvider(Func<object, object> handle) =>
        new ServiceCollection()
            .AddLogging()
            .AddScoped<IDispatcher>(_ => new StubDispatcher(handle))
            .AddMassTransitTestHarness(x =>
            {
                x.AddConsumer<GenerateEmailConfirmationTokenConsumer>();
                x.AddConsumer<GeneratePasswordResetTokenConsumer>();
            })
            .BuildServiceProvider(true);

    private static async Task<ITestHarness> StartAsync(ServiceProvider provider)
    {
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();
        return harness;
    }

    /// <summary>Runs a delegate in place of the real command pipeline (handlers need Identity + DB).</summary>
    private sealed class StubDispatcher(Func<object, object> handle) : IDispatcher
    {
        public Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
            where TCommand : class, ICommand
            => throw new NotSupportedException();

        public Task<TResult> SendAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
            where TCommand : class, ICommand<TResult>
        {
            try
            {
                return Task.FromResult((TResult)handle(command));
            }
            catch (Exception ex)
            {
                return Task.FromException<TResult>(ex);
            }
        }

        public Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
