using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using Core.DTO.UserSetting.Request;
using Core.DTO.UserSetting.Response;

namespace Core.Services
{
    public interface IUserSettingService
    {
        Task<UserSettingResponse> GetSettingsAsync(string userId);
        Task<UserSettingResponse> UpdateSettingsAsync(string userId, UserSettingRequest request);
    }
}
