using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tr_core.Entities;
using tr_core.Enums;

namespace tr_core.Repositories
{
    public interface IPlatformRepository : IRepository<Platform>
    {
        public Task<Platform?> GetPlatformByTypeAsync(PlatformType platformType);
    }
}
