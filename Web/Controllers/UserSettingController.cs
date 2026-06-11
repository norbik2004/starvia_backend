using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Core.DTO.UserSetting.Request;
using Core.DTO.UserSetting.Response;
using Core.Services;
using Service.Services;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserSettingController(IUserSettingService userSettingService) : ControllerBase
    {
        [Authorize]
        [HttpGet("settings")]
        [ProducesResponseType(typeof(UserSettingResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<UserSettingResponse> GetMySettings()
        {
            var userId = Helpers.UserHelpers.GetUserIdFromClaims(User);
            return await userSettingService.GetSettingsAsync(userId);
        }

        [Authorize]
        [HttpPut("settings")]
        [ProducesResponseType(typeof(UserSettingResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<UserSettingResponse> UpdateMySettings([FromBody] UserSettingRequest request)
        {
            var userId = Helpers.UserHelpers.GetUserIdFromClaims(User);
            return await userSettingService.UpdateSettingsAsync(userId, request);
        }
    }
}
