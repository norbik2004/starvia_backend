using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Media.Application.MediaFiles.Commands.UpdateFile;

public sealed record UpdateFileCommand(UpdateFileRequest request, Guid UserId) : ICommand<UpdateFileResult>;

public sealed record UpdateFileRequest(Guid FileId, string? Description);

public sealed record UpdateFileResult(Guid FileId);
