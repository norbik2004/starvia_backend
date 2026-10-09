using StarviaBackend.Shared.Abstractions.Exceptions;

namespace StarviaBackend.Modules.MediaFiles.Core.Media.Exceptions;


internal sealed class MediaNotFoundException(Guid mediaId)
    : BusinessException($"Media with {mediaId} was not found")
{
    public override string Code => "media_not_found";
}
