using StarviaBackend.Modules.Ai.Core.Chats.Repositories;
using StarviaBackend.Shared.Abstractions.Commands;
using StarviaBackend.Shared.Abstractions.Time;

namespace StarviaBackend.Modules.Ai.Application.Chats.Commands.AddChatMessage;

internal sealed class AddChatMessageHandler(IChatRepository chatRepository, IClock clock)
    : ICommandHandler<AddChatMessageCommand, AddChatMessageResult>
{
    public async Task<AddChatMessageResult> HandleAsync(
        AddChatMessageCommand command,
        CancellationToken cancellationToken = default)
    {
        var chat = await chatRepository.GetOwnedAsync(command.ChatId, command.UserId, cancellationToken);

        var message = chat.AddUserMessage(command.Request.Content, clock.UtcNow);

        await chatRepository.UpdateAsync(chat, cancellationToken);

        return new AddChatMessageResult(message.Id, message.Sequence);
    }
}
