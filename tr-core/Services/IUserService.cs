using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tr_core.DTO.User.Request;
using tr_core.DTO.User.Response;
using tr_core.Entities;

namespace tr_core.Services
{
    public interface IUserService
    {
        public Task RegisterUserAsync(UserRegisterRequest request);
        public Task<UserResponse> GetLoggedInUserInfoAsync(string userId);
        public Task<List<UserResponse>> GetAllUsers(UserPaginatedParamsRequest request);
        public Task SetStripeCustomerId(string userId, string customerId);
        public Task UpdateSubsciptionStatus(string userId, bool status);
        /*
        public Task<bool> CanGeneratePostAsync(string userId);
        public Task IncrementPostCounterAsync(string userId);
        */
    }
}
