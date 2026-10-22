using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Media.Application.MediaFiles.Queries.DownloadFile;

internal sealed record DownloadFileQuery(Guid UserId, Guid FileId) : IQuery<FileContentDto>;

public sealed record FileContentDto(Stream Content, string FileName);
