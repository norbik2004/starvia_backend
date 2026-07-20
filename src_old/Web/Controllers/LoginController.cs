using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Core.Application.DTO.Platform.Response;
using Core.Application.DTO.User.Request;
using Core.Domain.Entities;
using Service.Exceptions;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController(SignInManager<User> signInManager) : ControllerBase
    {

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LogIn([FromBody] UserLoginRequest request)
        {
            if (User.Identities.Any(i => i.IsAuthenticated))
                throw new BadRequestException("User is arelady logged in");

            var user = await signInManager.UserManager.FindByEmailAsync(request.Email)
                ?? throw new UnauthorizedException("Wrong password or email");

            var result = await signInManager.PasswordSignInAsync(
                user,
                request.Password,
                isPersistent: true,
                lockoutOnFailure: false
            );

            if (!result.Succeeded)
                throw new UnauthorizedException("Wrong password or email");

            return Ok();
        }

        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> LogOut()
        {
            if(User == null)
                throw new BadRequestException("User is not logged in");

            await signInManager.SignOutAsync();
            return Ok();
        }
    }
}
