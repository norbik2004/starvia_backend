using Microsoft.AspNetCore.Mvc;
using Core.DTO.Platform.Response;
using Core.DTO.Post.Response;
using Core.Services;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlatformController(IPlatformService platformService) : ControllerBase
    {
        [HttpGet("platforms")]
        [ProducesResponseType(typeof(List<PlatformResponse>), StatusCodes.Status200OK)]
        public async Task<List<PlatformResponse>> GetAllAsync()
        {
            return await platformService.GetAllAsync();
        }
    }
}
