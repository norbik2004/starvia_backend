using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DTO.UserPlatform.Request;
using Core.DTO.UserPlatform.Response;
using Core.Entities;
using Core.Repositories;
using Core.Services;
using Service.Exceptions;

namespace Service.Services
{
    public class UserPlatformService(IUserPlatformRepository userPlatformRepository, IPlatformRepository platformRepository,
        IMapper mapper) : IUserPlatformService
    {
        public async Task<UserPlatformResponse> AddUserPlatformAsync(UserPlatformRequest request, string userId)
        {
            var platform = await platformRepository.GetByIdAsync(request.PlatformId.ToString())
                ?? throw new BadRequestException("Platform not found");

            var exists = await userPlatformRepository.ExistsAsync(userId, request.PlatformId);

            if (exists)
                throw new BadRequestException("User platform already exists");

            var userPlatformEntity = mapper.Map<UserPlatform>(request);
            userPlatformEntity.UserId = userId;
            userPlatformEntity.PlatformId = platform.Id;
            userPlatformEntity.Platform = platform;

            await userPlatformRepository.AddAsync(userPlatformEntity);
            await userPlatformRepository.SaveChangesAsync();

            return mapper.Map<UserPlatformResponse>(userPlatformEntity);
        }

        public async Task<UserPlatformResponseLong> GetUserPlatformByIdAsync(int userPlatformId, string userId)
        {
            var platform = await userPlatformRepository.GetUserPlatformPerUserByIdAsync(userPlatformId, userId)
                ?? throw new NotFoundException("User platform not found");

            return mapper.Map<UserPlatformResponseLong>(platform);
        }

        public async Task<List<UserPlatformResponse>> GetUserPlatformsAsync(string userId)
        {
            var userPlatforms = await userPlatformRepository.GetUserPlatformsPerUserAsync(userId);

            return mapper.Map<List<UserPlatformResponse>>(userPlatforms);
        }

        public async Task RemoveUserPlatform(int userPlatformId, string userId)
        {
            var platform = await userPlatformRepository.GetUserPlatformPerUserByIdAsync(userPlatformId, userId)
                ?? throw new NotFoundException("User platform not found");

            userPlatformRepository.Remove(platform);
            await userPlatformRepository.SaveChangesAsync();
        }

        public async Task<UserPlatformResponseLong> UpdateUserPlatformAsync(int userPlatformId, UserPlatformUpdateRequest request, string userId)
        {
            var platform = await userPlatformRepository.GetUserPlatformPerUserByIdAsync(userPlatformId, userId)
                ?? throw new NotFoundException("User platform not found");

            var platformEntity = mapper.Map(request, platform);
            platformEntity.Id = userPlatformId;

            userPlatformRepository.Update(platformEntity);
            await userPlatformRepository.SaveChangesAsync();

            return mapper.Map<UserPlatformResponseLong>(platform);
        }
    }
}
