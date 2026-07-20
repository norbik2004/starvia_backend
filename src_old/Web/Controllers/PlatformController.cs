using Microsoft.AspNetCore.Mvc;
using Core.Application.DTO.Platform.Response;
using Core.Application.DTO.Post.Response;
using Core.Application.Services;

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
