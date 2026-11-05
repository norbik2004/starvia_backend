using StarviaBackend.Modules.Media.Core.Media.Enums;
using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Media.Application.MediaFiles.Queries.GetFile;

/// <summary>One file by id, if the caller may view it (see the media access policy).</summary>
internal sealed record GetFileQuery(Guid FileId) : IQuery<FileLongDto>;

public sealed record FileLongDto(Guid Id, string FileName, string? Description, FileSource Source, FileType Type,
    string Url, DateTime CreatedAt, string? CreatedBy);
