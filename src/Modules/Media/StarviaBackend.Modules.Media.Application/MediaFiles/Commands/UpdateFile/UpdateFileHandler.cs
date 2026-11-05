using StarviaBackend.Modules.Media.Application.MediaFiles.Access;
using StarviaBackend.Modules.Media.Core.Media.Access;
using StarviaBackend.Modules.Media.Core.Media.Exceptions;
using StarviaBackend.Modules.Media.Core.Media.Repositories;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Media.Application.MediaFiles.Commands.UpdateFile;

internal sealed class UpdateFileHandler(IMediaFileRepository mediaFileRepository, IMediaAccessService access)
    : ICommandHandler<UpdateFileCommand, UpdateFileResult>
{
    public async Task<UpdateFileResult> HandleAsync(UpdateFileCommand command, CancellationToken cancellationToken = default)
    {
        var file = await mediaFileRepository.GetByIdAsync(command.request.FileId, cancellationToken)
            ?? throw new MediaNotFoundException(command.request.FileId);

        access.EnsureAllowed(MediaAction.Update, file);

        file.UpdateDescription(command.request.Description);

        await mediaFileRepository.UpdateAsync(file, cancellationToken);

        return new UpdateFileResult(file.Id);
    }
}
