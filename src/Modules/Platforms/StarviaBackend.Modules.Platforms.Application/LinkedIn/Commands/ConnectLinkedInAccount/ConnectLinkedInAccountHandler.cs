using StarviaBackend.Modules.Platforms.Core.Platforms.Entities;
using StarviaBackend.Modules.Platforms.Core.Platforms.Exceptions;
using StarviaBackend.Modules.Platforms.Core.Platforms.Repositories;
using StarviaBackend.Modules.Platforms.Integrations.LinkedIn;
using StarviaBackend.Shared.Abstractions.Commands;
using StarviaBackend.Shared.Abstractions.Platforms;

namespace StarviaBackend.Modules.Platforms.Application.LinkedIn.Commands.ConnectLinkedInAccount;

internal sealed class ConnectLinkedInAccountHandler(
    IPlatformRepository platformRepository,
    IUserPlatformRepository userPlatformRepository,
    ILinkedInClient linkedInClient)
    : ICommandHandler<ConnectLinkedInAccountCommand, ConnectLinkedInAccountResult>
{
    public async Task<ConnectLinkedInAccountResult> HandleAsync(
        ConnectLinkedInAccountCommand command,
        CancellationToken cancellationToken = default)
    {
        if (!LinkedInOAuthState.TryParse(command.State, out var userId, out var platformId))
        {
            throw new LinkedInOAuthStateException();
        }

        var platform = await platformRepository.GetByIdAsync(platformId, cancellationToken)
            ?? throw new PlatformNotFoundException(platformId);

        if (platform.PlatformType != PlatformType.LinkedIn)
        {
            throw new LinkedInOAuthStateException();
        }

        if (!platform.IsOnline)
        {
            throw new PlatformOfflineException(PlatformType.LinkedIn);
        }

        var accessToken = await linkedInClient.ExchangeCodeForAccessTokenAsync(command.Code, cancellationToken);
        var account = await linkedInClient.GetAccountInfoAsync(accessToken, cancellationToken);

        var userPlatform = UserPlatform.Create(
            userId,
            platform.Id,
            account.Name,
            accessToken,
            account.Sub,
            accountComment: null,
            account.ProfilePictureUrl);

        await userPlatformRepository.AddAsync(userPlatform);

        return new ConnectLinkedInAccountResult(userPlatform.Id, linkedInClient.SuccessRedirectUrl);
    }
}
