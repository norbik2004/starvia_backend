using StarviaBackend.Modules.Ai.Application.Chats.Access;
using StarviaBackend.Modules.Ai.Core.Chats.Access;
using StarviaBackend.Modules.Ai.Core.Chats.Repositories;
using StarviaBackend.Shared.Abstractions.Commands;
using StarviaBackend.Shared.Abstractions.Time;

namespace StarviaBackend.Modules.Ai.Application.Chats.Commands.AddChatMessage;

internal sealed class AddChatMessageHandler(IChatRepository chatRepository, IChatAccessService access, IClock clock)
    : ICommandHandler<AddChatMessageCommand, AddChatMessageResult>
{
    public async Task<AddChatMessageResult> HandleAsync(
        AddChatMessageCommand command,
        CancellationToken cancellationToken = default)
    {
        var chat = await chatRepository.GetAllowedAsync(access, ChatAction.AddMessage, command.ChatId, cancellationToken);

        var message = chat.AddUserMessage(command.Request.Content, clock.UtcNow);

        await chatRepository.UpdateAsync(chat, cancellationToken);

        return new AddChatMessageResult(message.Id, message.Sequence);
    }
}
