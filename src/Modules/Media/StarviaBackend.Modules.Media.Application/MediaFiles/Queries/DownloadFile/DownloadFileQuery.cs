using StarviaBackend.Shared.Abstractions.Queries;

namespace StarviaBackend.Modules.Media.Application.MediaFiles.Queries.DownloadFile;

/// <summary>The content of one file, if the caller may download it (see the media access policy).</summary>
internal sealed record DownloadFileQuery(Guid FileId) : IQuery<FileContentDto>;

public sealed record FileContentDto(Stream Content, string FileName);
