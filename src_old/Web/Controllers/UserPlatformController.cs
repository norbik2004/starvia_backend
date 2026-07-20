using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Web.Helpers;
using Core.Application.DTO.UserPlatform.Request;
using Core.Application.DTO.UserPlatform.Response;
using Core.Application.Services;
using Service.Exceptions;

namespace Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserPlatformController(IUserPlatformService userPlatformService) : ControllerBase
    {
        [HttpPost("add")]
        [ProducesResponseType(typeof(UserPlatformResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<UserPlatformResponse> AddUserPlatform([FromBody] UserPlatformRequest request)
        {
            var userId = UserHelpers.GetUserIdFromClaims(User);

            return await userPlatformService.AddUserPlatformAsync(request, userId);
        }

        [HttpGet("user-platforms")]
        [ProducesResponseType(typeof(List<UserPlatformResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<List<UserPlatformResponse>> GetUserPlatformsPerUser()
        {
            var userId = UserHelpers.GetUserIdFromClaims(User);

            return await userPlatformService.GetUserPlatformsAsync(userId);
        }

        [HttpGet("{userPlatformId:int}")]
        [ProducesResponseType(typeof(UserPlatformResponseLong), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<UserPlatformResponseLong> GetUserPlatformsPerUser(int userPlatformId)
        {
            var userId = UserHelpers.GetUserIdFromClaims(User);

            return await userPlatformService.GetUserPlatformByIdAsync(userPlatformId, userId);
        }

        [HttpDelete("{userPlatformId:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveUserPlatform(int userPlatformId)
        {
            var userId = UserHelpers.GetUserIdFromClaims(User);

            await userPlatformService.RemoveUserPlatform(userPlatformId, userId);
            return NoContent();
        }

        [HttpPut("{userPlatformId:int}")]
        [ProducesResponseType(typeof(UserPlatformResponseLong), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<UserPlatformResponse> UpdateUserPlatform(int userPlatformId, [FromBody] UserPlatformUpdateRequest request)
        {
            var userId = UserHelpers.GetUserIdFromClaims(User);

            return await userPlatformService.UpdateUserPlatformAsync(userPlatformId, request, userId);
        }
    }
}
