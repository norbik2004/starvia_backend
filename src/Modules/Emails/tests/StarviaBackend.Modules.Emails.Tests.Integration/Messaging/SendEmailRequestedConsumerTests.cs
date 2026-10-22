using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Emails.Api;
using StarviaBackend.Modules.Emails.Application.Emails.Commands.SendEmail;
using StarviaBackend.Modules.Emails.Application.Emails.Events.SendEmailRequested;
using StarviaBackend.Modules.Emails.Core.Emails.Exceptions;
using StarviaBackend.Modules.Emails.Tests.Integration.Support;
using StarviaBackend.Shared.Abstractions.Dispatchers;
using StarviaBackend.Shared.Abstractions.Email;

namespace StarviaBackend.Modules.Emails.Tests.Integration.Messaging;

/// <summary>
/// Other modules publish <see cref="SendEmailRequestedEvent"/>; Emails turns it into a
/// <see cref="SendEmailCommand"/>. These tests run the consumer exactly as the Emails module
/// registers it (<see cref="EmailsModule.RegisterEmailsConsumers"/>, retry policy included) on the
/// in-memory test harness, with a recording dispatcher in place of the real handlers.
/// </summary>
public sealed class SendEmailRequestedConsumerTests
{
    [Fact]
    public async Task A_requested_email_is_dispatched_as_a_SendEmailCommand()
    {
        var userId = Guid.NewGuid();
        var dispatcher = new RecordingDispatcher { CommandResult = _ => new SendEmailResult(Guid.NewGuid()) };
        await using var provider = BuildProvider(dispatcher);
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        await harness.Bus.Publish(new SendEmailRequestedEvent(userId, "user@example.com", EmailType.ConfirmAccountEmail));

        (await harness.Consumed.Any<SendEmailRequestedEvent>()).Should().BeTrue();
        var command = dispatcher.Commands.Should().ContainSingle().Which.Should().BeOfType<SendEmailCommand>().Subject;
        command.UserId.Should().Be(userId);
        command.request.Email.Should().Be("user@example.com");
        command.request.EmailType.Should().Be(EmailType.ConfirmAccountEmail);
    }

    [Theory]
    [InlineData(EmailType.WelcomingEmail)]
    [InlineData(EmailType.ConfirmAccountEmail)]
    [InlineData(EmailType.ResetPasswordEmail)]
    public async Task Every_email_type_is_forwarded_unchanged(EmailType type)
    {
        var dispatcher = new RecordingDispatcher { CommandResult = _ => new SendEmailResult(Guid.NewGuid()) };
        await using var provider = BuildProvider(dispatcher);
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        await harness.Bus.Publish(new SendEmailRequestedEvent(Guid.NewGuid(), "user@example.com", type));

        (await harness.Consumed.Any<SendEmailRequestedEvent>()).Should().BeTrue();
        dispatcher.Commands.Should().ContainSingle()
            .Which.Should().BeOfType<SendEmailCommand>()
            .Which.request.EmailType.Should().Be(type);
    }

    [Fact]
    public async Task A_rejected_account_token_is_not_retried_and_goes_straight_to_the_error_queue()
    {
        // The retry policy waits a minute between attempts; if the rejection were retried the
        // fault would not show up within the harness timeout and this test would fail.
        var userId = Guid.NewGuid();
        var dispatcher = new RecordingDispatcher
        {
            CommandFailure = new AccountTokenRejectedException(userId, "user_not_found", "gone"),
        };
        await using var provider = BuildProvider(dispatcher);
        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        await harness.Bus.Publish(new SendEmailRequestedEvent(userId, "user@example.com", EmailType.ConfirmAccountEmail));

        (await harness.Published.Any<Fault<SendEmailRequestedEvent>>()).Should().BeTrue();
        dispatcher.Commands.Should().HaveCount(1);
    }

    private static ServiceProvider BuildProvider(RecordingDispatcher dispatcher) =>
        new ServiceCollection()
            .AddLogging()
            .AddSingleton<IDispatcher>(dispatcher)
            .AddMassTransitTestHarness(x => x.RegisterEmailsConsumers())
            .BuildServiceProvider(true);
}
