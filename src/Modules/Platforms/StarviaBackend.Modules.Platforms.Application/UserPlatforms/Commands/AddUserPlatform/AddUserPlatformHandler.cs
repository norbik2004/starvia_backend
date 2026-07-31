using MassTransit;
using Microsoft.AspNetCore.Identity;
using StarviaBackend.Modules.Platforms.Core.Platforms.Entities;
using StarviaBackend.Modules.Platforms.Core.Platforms.Exceptions;
using StarviaBackend.Modules.Platforms.Core.Platforms.Repositories;
using StarviaBackend.Shared.Abstractions.Commands;
using StarviaBackend.Shared.Abstractions.Contexts;
using StarviaBackend.Shared.Abstractions.Time;

namespace StarviaBackend.Modules.Platforms.Application.UserPlatforms.Commands.AddUserPlatform;

internal sealed class AddUserPlatformHandler(
    IUserPlatformRepository userPlatformRepository, IPlatformRepository platformRepository)
    : ICommandHandler<AddUserPlatformCommand, AddUserPlatformResult>
{
    public async Task<AddUserPlatformResult> HandleAsync(
        AddUserPlatformCommand command,
        CancellationToken cancellationToken = default)
    {
        _ = await platformRepository.GetByIdAsync(command.request.PlatformId, cancellationToken)
            ?? throw new PlatformNotFoundException(command.request.PlatformId);

        var userPlatform = UserPlatform.Create(
            command.UserId,
            command.request.PlatformId,
            command.request.AccountUserName,
            command.request.AccountComment);

        await userPlatformRepository.AddAsync(userPlatform);

        return new AddUserPlatformResult(userPlatform.Id);
    }
}
