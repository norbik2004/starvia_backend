using Core;
using Core.Application;
using Core.Application.DTO.API;
using Core.Application.DTO.User.Request;
using Core.Application.DTO.User.Response;
using Core.Application.Services;
using Core.Domain.Consts;
using Core.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Service.Exceptions;
using Service.Mapping;
using System.Security.Claims;
using Web.Helpers;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController(IUserService userService, IOptions<ApplicationSettings> appSettings) : ControllerBase
    {

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Register([FromBody] UserRegisterRequest request)
        {
            await userService.RegisterUserAsync(request);
            return Ok();
        }

        [Authorize]
        [HttpGet("me")]
        [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<UserResponse> GetLoggedInUserInfo()
        {
            var userId = UserHelpers.GetUserIdFromClaims(User);

            return await userService.GetLoggedInUserInfoAsync(userId);
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            var result = await userService.ConfirmEmailAsync(userId, token);

            if (!result.Success)
            {
                return Redirect($"{appSettings.Value.FrontendURL}/email-confirmed?status=error&message={Uri.EscapeDataString(result.Message)}");
            }

            return Redirect($"{appSettings.Value.FrontendURL}/email-confirmed?status=success");
        }

        [EnableRateLimiting("email-confirm")]
        [HttpGet("resend-confirmation-email")]
        public async Task<IActionResult> ResendConfirmationEmail([FromQuery] string email)
        {
            await userService.ResendConfirmationEmailAsync(email);
            return Ok();
        }

        [EnableRateLimiting("email-confirm")]
        [HttpGet("reset-password")]
        public async Task<IActionResult> SendResetPasswordEmail([FromQuery] string email)
        {
            await userService.SendPasswordResetEmailAsync(email);
            return Ok();
        }

        [HttpPost("confirm-reset-password")]
        public async Task<IActionResult> ValidateResetPasswordRequest([FromQuery] string token, [FromQuery] string userId, [FromBody] string password)
        {
            var result = await userService.ResetPassword(userId, token, password);

            if (!result.Success)
            {
                throw new BadRequestException("Error while resetting password, contact support");
            }

            return Ok("Password has been changed succesfully");
        }

        [HttpPut]
        [Authorize]
        public async Task<UserResponse> ChangeUsername([FromBody] string username)
        {
            var userId = UserHelpers.GetUserIdFromClaims(User);

            return await userService.UpdateUsername(userId, username);
        }
    
    }
}
