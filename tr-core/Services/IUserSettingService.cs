using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using tr_core.DTO.UserSetting.Request;
using tr_core.DTO.UserSetting.Response;

namespace tr_core.Services
{
    public interface IUserSettingService
    {
        Task<UserSettingResponse> GetSettingsAsync(string userId);
        Task<UserSettingResponse> UpdateSettingsAsync(string userId, UserSettingRequest request);
    }
}
