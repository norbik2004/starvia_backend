using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Media.Application.MediaFiles.Commands.AddFile;

public sealed record AddFileCommand(AddFileRequest request, Guid UserId, string UserEmail) : ICommand<AddFileResult>;

public sealed record AddFileRequest(
    string FileName,
    string ContentType,
    long Length,
    Stream Content,
    string? Description);

public sealed record AddFileResult(Guid FileId, string FileName, string FilePath);
