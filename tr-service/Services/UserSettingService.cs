using AutoMapper;
using System.Threading.Tasks;
using tr_core.DTO.UserSetting.Request;
using tr_core.DTO.UserSetting.Response;
using tr_core.Entities;
using tr_core.Repositories;
using tr_core.Services;
using tr_service.Exceptions;

namespace tr_service.Services
{
    public class UserSettingService(IUserSettingRepository userSettingRepository, IMapper mapper) : IUserSettingService
    {
        public async Task<UserSettingResponse> GetSettingsAsync(string userId)
        {
            var settings = await userSettingRepository.GetByUserIdAsync(userId);
            if (settings == null)
                throw new NotFoundException("User settings not found");

            return mapper.Map<UserSettingResponse>(settings);
        }

        public async Task<UserSettingResponse> UpdateSettingsAsync(string userId, UserSettingRequest request)
        {
            var settings = await userSettingRepository.GetByUserIdAsync(userId);

            if (settings == null)
            {
                settings = mapper.Map<UserSetting>(request);
                settings.UserId = userId;
                await userSettingRepository.AddAsync(settings);
                await userSettingRepository.SaveChangesAsync();
                return mapper.Map<UserSettingResponse>(settings);
            }

            mapper.Map(request, settings);
            userSettingRepository.Update(settings);
            await userSettingRepository.SaveChangesAsync();

            return mapper.Map<UserSettingResponse>(settings);
        }
    }
}
