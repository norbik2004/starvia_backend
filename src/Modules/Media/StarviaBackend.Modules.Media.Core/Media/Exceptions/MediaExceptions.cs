using StarviaBackend.Shared.Abstractions.Exceptions;

namespace StarviaBackend.Modules.Media.Core.Media.Exceptions;


internal sealed class MediaNotFoundException(Guid mediaId)
    : BusinessException($"Media with {mediaId} was not found")
{
    public override string Code => "media_not_found";
}

internal sealed class MediaFileContentNotFoundException(string key)
    : BusinessException($"Content of media file '{key}' was not found in the storage")
{
    public override string Code => "media_file_content_not_found";
}
