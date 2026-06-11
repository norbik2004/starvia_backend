using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DTO.Platform.Response;
using Core.Enums;
using Core.Repositories;
using Core.Services;
using Service.Exceptions;

namespace Service.Services
{
    public class PlatformService(IPlatformRepository repository, IMapper mapper) : IPlatformService
    {
        public async Task<List<PlatformResponse>> GetAllAsync()
        {
            var platforms = await repository.GetAllAsync();

            return mapper.Map<List<PlatformResponse>>(platforms);
        }

        public async Task<PlatformResponse> GetByPlatformTypeAsync(PlatformType platformType)
        {
            var platform = await repository.GetPlatformByTypeAsync(platformType);

            if(platform == null)
            {
                throw new BadRequestException($"Platform with type {platformType} not found.");
            }

            return mapper.Map<PlatformResponse>(platform);
        }
    }
}
