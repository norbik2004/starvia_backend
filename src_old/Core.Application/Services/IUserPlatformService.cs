using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Application.DTO.UserPlatform.Request;
using Core.Application.DTO.UserPlatform.Response;

namespace Core.Application.Services
{
    public interface IUserPlatformService
    {
        Task<UserPlatformResponse> AddUserPlatformAsync(UserPlatformRequest request, string userId);
        Task<List<UserPlatformResponse>> GetUserPlatformsAsync(string userId);
        Task<UserPlatformResponseLong> GetUserPlatformByIdAsync(int userPlatformId, string userId);
        Task RemoveUserPlatform(int userPlatformId, string userId);
        Task<UserPlatformResponse> UpdateUserPlatformAsync(int userPlatformId, UserPlatformUpdateRequest request, string userId);
    }
}
