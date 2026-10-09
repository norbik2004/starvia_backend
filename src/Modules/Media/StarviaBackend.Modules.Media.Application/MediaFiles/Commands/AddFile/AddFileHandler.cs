using StarviaBackend.Modules.Media.Core.Media.Entities;
using StarviaBackend.Modules.Media.Core.Media.Enums;
using StarviaBackend.Modules.Media.Core.Media.Repositories;
using StarviaBackend.Modules.Media.Core.Media.Storage;
using StarviaBackend.Shared.Abstractions.Commands;
using StarviaBackend.Shared.Abstractions.Storage;

namespace StarviaBackend.Modules.Media.Application.MediaFiles.Commands.AddFile;

internal sealed class AddFileHandler(IMediaFileRepository mediaFileRepository, IFileStorageService fileStorage)
    : ICommandHandler<AddFileCommand, AddFileResult>
{
    public async Task<AddFileResult> HandleAsync(AddFileCommand command, CancellationToken cancellationToken = default)
    {
        var request = command.request;

        var bucketName = BucketNames.UserUploads;

        var fileName = Path.GetFileName(request.FileName);
        var key = BuildStorageKey(command.UserEmail, fileName);

        await fileStorage.UploadAsync(key, request.Content, request.ContentType, bucketName, cancellationToken);

        try
        {
            var mediaFile = MediaFile.Create(
                fileName,
                key,
                FileSource.User,
                ResolveFileType(request.ContentType),
                request.Description);

            await mediaFileRepository.AddAsync(mediaFile, cancellationToken);

            return new AddFileResult(mediaFile.Id, mediaFile.FileName, mediaFile.FilePath);
        }
        catch
        {
            // Delete form bucket the when saving to database goes wrong
            await fileStorage.DeleteAsync(key, bucketName, CancellationToken.None);
            throw;
        }
    }

    private static string BuildStorageKey(string userEmail, string fileName)
    {
        var name = Path.GetFileNameWithoutExtension(fileName);
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return $"{userEmail}/{name}_{Guid.NewGuid():N}{extension}";
    }

    private static FileType ResolveFileType(string contentType)
    {
        var type = contentType.Trim().ToLowerInvariant();

        return type switch
        {
            _ when type.StartsWith("image/") => FileType.Image,
            _ when type.StartsWith("video/") => FileType.Video,
            _ when type.StartsWith("audio/") => FileType.Audio,
            _ when type.StartsWith("text/")
                   || type == "application/pdf"
                   || type == "application/msword"
                   || type.StartsWith("application/vnd.openxmlformats-officedocument")
                   || type.StartsWith("application/vnd.ms-") => FileType.Document,
            _ => FileType.Other
        };
    }
}
