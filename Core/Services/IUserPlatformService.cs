using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DTO.UserPlatform.Request;
using Core.DTO.UserPlatform.Response;

namespace Core.Services
{
    public interface IUserPlatformService
    {
        Task<UserPlatformResponse> AddUserPlatformAsync(UserPlatformRequest request, string userId);
        Task<List<UserPlatformResponse>> GetUserPlatformsAsync(string userId);
        Task<UserPlatformResponseLong> GetUserPlatformByIdAsync(int userPlatformId, string userId);
        Task RemoveUserPlatform(int userPlatformId, string userId);
        Task<UserPlatformResponseLong> UpdateUserPlatformAsync(int userPlatformId, UserPlatformUpdateRequest request, string userId);
    }
}
