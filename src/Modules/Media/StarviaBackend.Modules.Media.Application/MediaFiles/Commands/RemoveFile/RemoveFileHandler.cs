using StarviaBackend.Modules.Media.Core.Media.Exceptions;
using StarviaBackend.Modules.Media.Core.Media.Repositories;
using StarviaBackend.Modules.Media.Core.Media.Storage;
using StarviaBackend.Shared.Abstractions.Commands;
using StarviaBackend.Shared.Abstractions.Storage;

namespace StarviaBackend.Modules.Media.Application.MediaFiles.Commands.RemoveFile;

internal sealed class RemoveFileHandler(IMediaFileRepository mediaFileRepository, IFileStorageService fileStorageService)
    : ICommandHandler<RemoveFileCommand>
{
    public const string BucketName = BucketNames.UserUploads;

    public async Task HandleAsync(RemoveFileCommand command, CancellationToken cancellationToken = default)
    {
        var file = await mediaFileRepository.GetByIdAsync(command.FileId, cancellationToken)
            ?? throw new MediaNotFoundException(command.FileId);

        if(file.CreatedBy != command.UserId.ToString())
        {
            throw new MediaFileCannotBeDeletedException(file.Id);
        }

        try
        {
            await fileStorageService.DeleteAsync(file.FilePath, BucketName, cancellationToken);
        }
        catch (Exception)
        {
            throw new MediaFileCannotBeDeletedException(file.Id);
        }

        await mediaFileRepository.RemoveAsync(file, cancellationToken);
    }
}
