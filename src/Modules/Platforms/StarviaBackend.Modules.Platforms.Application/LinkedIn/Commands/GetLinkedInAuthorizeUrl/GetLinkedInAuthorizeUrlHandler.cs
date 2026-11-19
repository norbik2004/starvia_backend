using StarviaBackend.Modules.Platforms.Core.Platforms.Exceptions;
using StarviaBackend.Modules.Platforms.Core.Platforms.Repositories;
using StarviaBackend.Modules.Platforms.Integrations.LinkedIn;
using StarviaBackend.Shared.Abstractions.Commands;
using StarviaBackend.Shared.Abstractions.Platforms;

namespace StarviaBackend.Modules.Platforms.Application.LinkedIn.Commands.GetLinkedInAuthorizeUrl;

internal sealed class GetLinkedInAuthorizeUrlHandler(
    IPlatformRepository platformRepository,
    ILinkedInClient linkedInClient)
    : ICommandHandler<GetLinkedInAuthorizeUrlCommand, GetLinkedInAuthorizeUrlResult>
{
    public async Task<GetLinkedInAuthorizeUrlResult> HandleAsync(
        GetLinkedInAuthorizeUrlCommand command,
        CancellationToken cancellationToken = default)
    {
        var platform = await platformRepository.GetByTypeAsync(PlatformType.LinkedIn, cancellationToken)
            ?? throw new PlatformNotFoundException(PlatformType.LinkedIn);

        if (!platform.IsOnline)
        {
            throw new PlatformOfflineException(PlatformType.LinkedIn);
        }

        var url = linkedInClient.BuildAuthorizationUrl(command.UserId, platform.Id);
        return new GetLinkedInAuthorizeUrlResult(url);
    }
}
