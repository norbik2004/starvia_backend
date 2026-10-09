using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Modules.Media.Core.Media.Enums;
using StarviaBackend.Shared.Abstractions.Domain;

namespace StarviaBackend.Modules.Media.Core.Media.Entities;

internal sealed class MediaFile : AuditableEntity
{
    public string FileName { get; private set; }

    public string FilePath { get; private set; }
    public string? Description { get; private set; }
    public FileSource Source { get; private set; }
    public FileType Type { get; private set; }

    private MediaFile()
    {

    }

    public static MediaFile Create(string fileName, string filePath, FileSource source, FileType type, string? description)
    {
        return new MediaFile
        {
            Id = Guid.NewGuid(),
            FileName = fileName,
            FilePath = filePath,
            Description = description,
            Source = source,
            Type = type
        };
    }

    public void UpdateDescription(string? description)
    {
        Description = description;
    }

    public void Update(string fileName, string filePath, FileSource source, FileType type, string? description)
    {
        FileName = fileName;
        FilePath = filePath;
        Source = source;
        Type = type;
        Description = description;
    }
}
