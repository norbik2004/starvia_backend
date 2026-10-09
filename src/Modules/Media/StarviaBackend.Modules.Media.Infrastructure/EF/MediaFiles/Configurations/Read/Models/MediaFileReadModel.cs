using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Modules.Media.Core.Media.Enums;

namespace StarviaBackend.Modules.Media.Infrastructure.EF.MediaFiles.Configurations.Read.Models;

internal sealed class MediaFileReadModel
{
    public Guid Id { get; init; }
    public string FilePath { get; init; }
    public string FileName { get; init; }
    public string? Description { get; init; }
    public FileSource Source { get; init; }
    public FileType Type { get; init; }
}
