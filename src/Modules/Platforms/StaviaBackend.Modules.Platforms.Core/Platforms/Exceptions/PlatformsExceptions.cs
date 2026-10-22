using StarviaBackend.Shared.Abstractions.Exceptions;

namespace StarviaBackend.Modules.Platforms.Core.Platforms.Exceptions;

internal sealed class PlatformNotFoundException(Guid platformId)
    : BusinessException($"Platform '{platformId}' was not found.")
{
    public override string Code => "platform_not_found";
}

