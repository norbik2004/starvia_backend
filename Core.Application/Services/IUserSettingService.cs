using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;
using Core.Application.DTO.UserSetting.Request;
using Core.Application.DTO.UserSetting.Response;

namespace Core.Application.Services
{
    public interface IUserSettingService
    {
        Task<UserSettingResponse> GetSettingsAsync(string userId);
        Task<UserSettingResponse> UpdateSettingsAsync(string userId, UserSettingRequest request);
    }
}
