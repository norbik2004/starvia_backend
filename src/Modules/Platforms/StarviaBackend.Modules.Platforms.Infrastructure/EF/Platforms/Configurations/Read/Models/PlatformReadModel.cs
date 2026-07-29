using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using StarviaBackend.Modules.Platforms.Core.Platforms.Enums;

namespace StarviaBackend.Modules.Platforms.Infrastructure.EF.Platforms.Configurations.Read.Models;

internal sealed class PlatformReadModel
{
    public Guid Id { get; init; }
    public PlatformType PlatformType { get; init; }
    public bool IsOnline { get; init; }
}
