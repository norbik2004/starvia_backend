using System.Runtime.CompilerServices;
using FluentAssertions;
using MassTransit;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using StarviaBackend.Modules.Accounts.Application.Users.Events.UserRegistered;

namespace StarviaBackend.Modules.Accounts.Tests.Integration.Messaging;

/// <summary>
/// Focused messaging test using MassTransit's in-memory test harness (no DB, no HTTP).
/// This is the pattern to copy when testing a module's consumers in isolation.
/// </summary>
public sealed class UserRegisteredMessagingTests
{
    [Fact]
    public async Task Publishing_UserRegisteredEvent_is_delivered_to_the_consumer()
    {
        await using var provider = new ServiceCollection()
            .AddLogging()
            .AddMassTransitTestHarness(x => x.AddConsumer<UserRegisteredConsumer>())
            .BuildServiceProvider(true);

        var harness = provider.GetRequiredService<ITestHarness>();
        await harness.Start();

        var @event = new UserRegisteredEvent(Guid.NewGuid(), "user@example.com", DateTime.UtcNow);
        await harness.Bus.Publish(@event);

        (await harness.Published.Any<UserRegisteredEvent>()).Should().BeTrue();
        (await harness.Consumed.Any<UserRegisteredEvent>()).Should().BeTrue();

        var consumerHarness = harness.GetConsumerHarness<UserRegisteredConsumer>();
        (await consumerHarness.Consumed.Any<UserRegisteredEvent>()).Should().BeTrue();
    }
}
