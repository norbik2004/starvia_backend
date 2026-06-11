using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Entities;
using Core.Enums;

namespace Core.Repositories
{
    public interface IPlatformRepository : IRepository<Platform>
    {
        public Task<Platform?> GetPlatformByTypeAsync(PlatformType platformType);
    }
}
