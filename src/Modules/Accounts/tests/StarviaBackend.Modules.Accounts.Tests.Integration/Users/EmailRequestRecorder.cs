using System.Collections.Concurrent;
using MassTransit;
using StarviaBackend.Shared.Abstractions.Email;

namespace StarviaBackend.Modules.Accounts.Tests.Integration.Users;

/// <summary>
/// Listens to everything published on the bus and keeps the <see cref="SendEmailRequestedEvent"/>s.
/// Unlike the harness' message lists it never blocks waiting for more traffic, so "nothing was
/// published" can be asserted instantly. Publishing done by an endpoint's handler has already been
/// observed when the HTTP response comes back; use <see cref="WaitForAsync"/> only for messages
/// produced by a consumer further down the chain.
/// </summary>
internal sealed class EmailRequestRecorder : IPublishObserver, IDisposable
{
    private readonly ConcurrentQueue<SendEmailRequestedEvent> _messages = new();
    private readonly ConnectHandle _handle;

    public EmailRequestRecorder(IBus bus) => _handle = bus.ConnectPublishObserver(this);

    public IReadOnlyList<SendEmailRequestedEvent> Messages => [.. _messages];

    public async Task<bool> WaitForAsync(Func<SendEmailRequestedEvent, bool> matches, TimeSpan? timeout = null)
    {
        var deadline = DateTime.UtcNow + (timeout ?? TimeSpan.FromSeconds(10));

        while (DateTime.UtcNow < deadline)
        {
            if (_messages.Any(matches))
            {
                return true;
            }

            await Task.Delay(25);
        }

        return _messages.Any(matches);
    }

    public Task PrePublish<T>(PublishContext<T> context)
        where T : class => Task.CompletedTask;

    public Task PostPublish<T>(PublishContext<T> context)
        where T : class
    {
        if (context.Message is SendEmailRequestedEvent message)
        {
            _messages.Enqueue(message);
        }

        return Task.CompletedTask;
    }

    public Task PublishFault<T>(PublishContext<T> context, Exception exception)
        where T : class => Task.CompletedTask;

    public void Dispose() => _handle.Disconnect();
}
