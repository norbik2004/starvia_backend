using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Application.DTO.Platform.Response;
using Core.Domain.Enums;

namespace Core.Application.Services
{
    public interface IPlatformService
    {
        public Task<List<PlatformResponse>> GetAllAsync();
        public Task<PlatformResponse> GetByPlatformTypeAsync(PlatformType platformType);
    }
}
