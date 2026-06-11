using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DTO.User.Request;
using Core.DTO.User.Response;
using Core.Entities;

namespace Core.Services
{
    public interface IUserService
    {
        public Task RegisterUserAsync(UserRegisterRequest request);
        public Task<UserResponse> GetLoggedInUserInfoAsync(string userId);
        public Task<List<UserResponse>> GetAllUsers(UserPaginatedParamsRequest request);
        public Task SetStripeCustomerId(string userId, string customerId);
        public Task UpdateSubsciptionStatus(string userId, bool status);
        public Task<bool> CanUserAccessAi(string userId);
        public Task ConfirmEmailAsync(string userId, string token);
        public Task ResendConfirmationEmailAsync(string email);
    }
}
