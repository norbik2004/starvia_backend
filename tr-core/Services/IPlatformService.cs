using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tr_core.DTO.Platform.Response;
using tr_core.Enums;

namespace tr_core.Services
{
    public interface IPlatformService
    {
        public Task<List<PlatformResponse>> GetAllAsync();
        public Task<PlatformResponse> GetByPlatformTypeAsync(PlatformType platformType);
    }
}
