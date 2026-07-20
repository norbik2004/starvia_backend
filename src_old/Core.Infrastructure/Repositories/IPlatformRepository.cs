using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Domain.Entities;
using Core.Domain.Enums;

namespace Core.Infrastructure.Repositories
{
    public interface IPlatformRepository : IRepository<Platform>
    {
        public Task<Platform?> GetPlatformByTypeAsync(PlatformType platformType);
    }
}
