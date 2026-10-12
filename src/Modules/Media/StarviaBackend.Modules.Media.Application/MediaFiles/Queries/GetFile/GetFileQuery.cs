using StarviaBackend.Modules.Media.Core.Media.Enums;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Media.Application.MediaFiles.Queries.GetFile;

internal sealed record GetFileQuery(Guid UserId, Guid FileId) : IQuery<FileLongDto>;

public sealed record FileLongDto(Guid Id, string FileName, string? Description, FileSource Source, FileType Type,
    string Url, DateTime CreatedAt);
