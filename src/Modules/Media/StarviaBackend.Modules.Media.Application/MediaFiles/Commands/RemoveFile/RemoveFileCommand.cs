using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Shared.Abstractions.Commands;

namespace StarviaBackend.Modules.Media.Application.MediaFiles.Commands.RemoveFile;

public sealed record RemoveFileCommand(Guid FileId, Guid UserId, string UserEmail) : ICommand;

public sealed record RemoveFileRequest(Guid FileId);
