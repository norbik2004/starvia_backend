using System;
using System.Collections.Generic;
using System.Text;
using StarviaBackend.Modules.Platforms.Core.Platforms.Entities;

namespace StarviaBackend.Modules.Platforms.Core.Platforms.Repositories;

internal interface IPlatformRepository
{
    Task<Platform?> GetByIdAsync(Guid platformId, CancellationToken cancellationToken = default);
}
