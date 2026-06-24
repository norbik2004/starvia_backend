using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Core.Domain.Consts;
using Core.Application.DTO.Email.Models;
using Core.Application.DTO.User.Request;
using Core.Application.DTO.User.Response;
using Core.Domain.Entities;
using Core.Domain.Enums;
using Core.Infrastructure.Repositories;
using Core.Application.Services;
using Core.Application.Services.Email;
using Repository.Migrations;
using Service.Exceptions;

namespace Service.Services
{
    public class UserService(UserManager<User> userManager, IUserRepository userRepository,
        IMapper mapper, IEmailSender emailSender) : IUserService
    {

        public async Task<bool> CanUserAccessAi(string userId)
        {
            var user = await userRepository.GetByIdAsync(userId);

            if (user == null)
                throw new BadRequestException($"User was not found, {nameof(user)}.");

            int count = user.UserPrompts.Count(c =>
                c.CreatedAt > DateTime.UtcNow.AddDays(-30));

            var userLimit = user.IsSubscribed ? AiCallsLimits.Free : AiCallsLimits.Subscribed;

            if (count >= userLimit)
                return false;

            return true;
        }

        public async Task ConfirmEmailAsync(string userId, string token)
        {
            var user = await userRepository.GetByIdAsync(userId)
                ?? throw new NotFoundException("Bad request");

            var result = await userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
            {
                throw new BadRequestException("Error, contact support");
            }
        }

        public async Task<List<UserResponse>> GetAllUsers(UserPaginatedParamsRequest request)
        {
            var users = await userRepository.GetAllAsync();

            var usersToReturn = mapper.Map<List<UserResponse>>(users);

            for (int i = 0; i < usersToReturn.Count; i++)
            {
                List<string> roles = [.. await userManager.GetRolesAsync(users[i])];
                usersToReturn[i].Roles = roles;
            }

            return usersToReturn;
        }

        public async Task<UserResponse> GetLoggedInUserInfoAsync(string userId)
        {
            var user = await userRepository.GetByIdAsync(userId)
                ?? throw new NotFoundException("User not found");

            List<string> roles = [.. await userManager.GetRolesAsync(user)];

            var userToReturn = mapper.Map<UserResponse>(user);
            userToReturn.Roles = roles;

            return userToReturn;
        }

        public async Task RegisterUserAsync(UserRegisterRequest request)
        {
            var existingUser = await userManager.FindByEmailAsync(request.Email);

            if (existingUser != null)
                throw new BadRequestException("Account with this email address arelady exists");

            var userName = request.Email.Split('@')[0];

            var user = new User
            {
                Email = request.Email,
                UserName = userName,
                NormalizedEmail = request.Email.ToUpper(),
                NormalizedUserName = userName.ToUpper(),
                EmailConfirmed = false
            };

            var result = await userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                throw new BadRequestException("Couldn't register user");
            }

            await userManager.AddToRoleAsync(user, Roles.User);

            var settings = new UserSetting
            {
                UserId = user.Id,
                IsDarkMode = true,
                ReceiveNotifications = false
            };

            user.UserSettings = settings;
            await userRepository.SaveChangesAsync();

            await SendConfirmationEmailAsync(user);
        }

        public async Task ResendConfirmationEmailAsync(string email)
        {
            var user = await userManager.FindByEmailAsync(email)
                ?? throw new NotFoundException("User not found");

            if (user.EmailConfirmed)
                throw new BadRequestException("Email is already confirmed");

            await SendConfirmationEmailAsync(user);
        }

        public async Task SetStripeCustomerId(string userId, string customerId)
        {
            var user = await userRepository.GetByIdAsync(userId)
                ?? throw new NotFoundException("User not found");

            user.StripeCustomerId = customerId;

            await userRepository.SaveChangesAsync();
        }

        public async Task UpdateSubsciptionStatus(string userId, bool status)
        {
            var user = await userRepository.GetByIdAsync(userId)
                ?? throw new NotFoundException("User not found");

            user.IsSubscribed = status;

            await userRepository.SaveChangesAsync();
        }

        private async Task SendConfirmationEmailAsync(User user)
        {
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

            ConfirmEmailRequest emailRequest = new()
            {
                To = user.Email!,
                Token = token,
                UserId = user.Id,
                EmailType = EmailForm.EmailConfirmation,
            };
            await emailSender.SendConfirmationEmail(emailRequest);
        }

    }
}
