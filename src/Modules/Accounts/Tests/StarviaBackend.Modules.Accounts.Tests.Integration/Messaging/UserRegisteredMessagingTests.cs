using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Accounts.Application.Users.Commands.GenerateEmailConfirmationToken;
using StarviaBackend.Modules.Accounts.Application.Users.Events.UserRegistered;
using StarviaBackend.Shared.Abstractions.App;
using StarviaBackend.Shared.Abstractions.Commands;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Email;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Accounts.Tests.Integration.Messaging;

/// <summary>
/// Focused messaging test using MassTransit's in-memory test harness (no DB, no HTTP).
/// This is the pattern to copy when testing a module's consumers in isolation.
/// </summary>
public sealed class UserRegisteredMessagingTests
{
    [Fact]
    public async Task Publishing_UserRegisteredEvent_queues_confirm_account_email()
    {
        await using var provider = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IDispatcher, StubDispatcher>()
            .AddSingleton<IAppUrls, StubAppUrls>()
            .AddMassTransitTestHarness(x => x.AddConsumer<UserRegisteredConsumer>())
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var @event = new UserRegisteredEvent(Guid.NewGuid(), "user@example.com", DateTime.UtcNow);
        await harness.Bus.Publish(@event);

        (await harness.Published.Any<UserRegisteredEvent>()).Should().BeTrue();
        (await harness.Consumed.Any<UserRegisteredEvent>()).Should().BeTrue();
        (await harness.Published.Any<SendEmailRequestedEvent>()).Should().BeTrue();

        var consumerHarness = harness.GetConsumerHarness<UserRegisteredConsumer>();
        (await consumerHarness.Consumed.Any<UserRegisteredEvent>()).Should().BeTrue();

        var publishedEmail = harness.Published.Select<SendEmailRequestedEvent>().First();
        publishedEmail.Context.Message.EmailType.Should().Be("ConfirmAccountEmail");
        publishedEmail.Context.Message.Link.Should().Contain("/email-confirmed");
        publishedEmail.Context.Message.Link.Should().Contain($"userId={@event.UserId}");
        publishedEmail.Context.Message.Link.Should().StartWith("http://front.test/");
    }

    private sealed class StubAppUrls : IAppUrls
    {
        public string ApiBaseUrl => "http://api.test";
        public string FrontendBaseUrl => "http://front.test";
    }

    private sealed class StubDispatcher : IDispatcher
    {
        public Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default)
            where TCommand : class, ICommand
            => Task.CompletedTask;

        public Task<TResult> SendAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
            where TCommand : class, ICommand<TResult>
        {
            if (typeof(TResult) == typeof(GenerateEmailConfirmationTokenResult))
            {
                object result = new GenerateEmailConfirmationTokenResult("test-token");
                return Task.FromResult((TResult)result);
            }

            throw new NotSupportedException($"Unexpected command result type {typeof(TResult).Name}");
        }

        public Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();
    }
}
