using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Application.DTO.API;
using Core.Application.DTO.User.Request;
using Core.Application.DTO.User.Response;
using Core.Domain.Entities;

namespace Core.Application.Services
{
    public interface IUserService
    {
        public Task RegisterUserAsync(UserRegisterRequest request);
        public Task<UserResponse> GetLoggedInUserInfoAsync(string userId);
        public Task<List<UserResponse>> GetAllUsers(UserPaginatedParamsRequest request);
        public Task SetStripeCustomerId(string userId, string customerId);
        public Task UpdateSubsciptionStatus(string userId, bool status);
        public Task<bool> CanUserAccessAi(string userId);
        public Task<ApiEmailResponse> ConfirmEmailAsync(string userId, string token);
        public Task ResendConfirmationEmailAsync(string email);
        public Task SendPasswordResetEmailAsync(string email);
        public Task<ApiEmailResponse> ResetPassword(string userId, string token, string password);
        public Task<UserResponse> UpdateUsername(string userId, string username);
    }
}
