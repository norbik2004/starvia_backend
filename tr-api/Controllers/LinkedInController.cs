using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tr_core.Services;
using tr_backend.Helpers;
using System.Threading.Tasks;
using tr_core.DTO.UserPlatform.Request;
using tr_service.LinkedIn;
using tr_core.DTO.LinkedIn;
using tr_core.DTO.LinkedIn.Request;
using tr_core.Enums;

namespace tr_backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LinkedInController(ILinkedInService linkedInService, IUserPlatformService userPlatformService, 
        IPlatformService platformService, LinkedInConfig enviromentalConfig, IConfiguration appsettingsConfig) : ControllerBase
    {

        [HttpGet("authorize")]
        public async Task<IActionResult> GetAuthorizationUrl()
        {
            var redirect = enviromentalConfig.RedirectUri;
            var clientId = enviromentalConfig.ClientId;
            var scopes = "openid%20profile%20w_member_social%20email";

            var linkedinPlatform = await platformService.GetByPlatformTypeAsync(PlatformType.LinkedIn);

            var state = $"{Guid.NewGuid()}|{linkedinPlatform.Id}";

            var url = $"{appsettingsConfig["LinkedIn:BaseUrl"]}authorization?response_type=code&client_id={clientId}" +
                $"&redirect_uri={Uri.EscapeDataString(redirect)}&scope={scopes}&state={Uri.EscapeDataString(state)}";

            return Ok(new { url });
        }

        [HttpGet("callback")]
        public async Task<IActionResult> Callback([FromQuery] string code, [FromQuery] string state)
        {
            var userId = UserHelpers.GetUserIdFromClaims(User);

            if (string.IsNullOrEmpty(state) || !state.Contains('|'))
                return BadRequest("Missing or invalid state parameter");

            var parts = state.Split('|');
            if (!int.TryParse(parts.Last(), out var platformId))
                return BadRequest("Invalid platform id in state");

            var accessToken = await linkedInService.ExchangeCodeForAccessToken(code, enviromentalConfig.RedirectUri);
            var accountInfo = await linkedInService.GetAccountInfo(accessToken);

            var request = new UserPlatformRequest
            {
                PlatformId = platformId,
                AccessToken = accessToken,
                ExternalAccountId = accountInfo.Sub,
                AccountUsername = accountInfo.Name,
                AccountComment = "",
                ProfilePictureLink = accountInfo.PFPurl
            };

            // Someday redirecting to frontend with UserPlatform object
            _ = await userPlatformService.AddUserPlatformAsync(request, userId);

            return Redirect("https://www.youtube.com/watch?v=dQw4w9WgXcQ&list=RDdQw4w9WgXcQ&start_radio=1");
        }

    }
}
