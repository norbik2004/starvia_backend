using Core.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Services;
using Web.Helpers;

namespace Web.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class ImagePrompFileController(IImagePromptFileService imagePromptFileService) : ControllerBase
    {
        [HttpGet("download/{fileId:Guid}")]
        public async Task<IActionResult> DownloadFile(Guid fileId)
        {
            var userId = UserHelpers.GetUserIdFromClaims(User);

            var file = await imagePromptFileService.DownloadFileById(fileId, userId);

            return file;
        }
    }
}
