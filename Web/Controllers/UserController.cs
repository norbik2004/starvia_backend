using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Core.Consts;
using Core.DTO.User.Request;
using Core.DTO.User.Response;
using Core.DTO.UserPlatform.Response;
using Core.Services;
using Service.Mapping;
using Service.Services;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IUserService userService, IMapper mapper) : ControllerBase
    {
        [Authorize(Roles = Roles.Admin)]
        [HttpGet("users")]
        [ProducesResponseType(typeof(PaginatedList<UserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<PaginatedList<UserResponse>> GetUsers([FromQuery] UserPaginatedParamsRequest request)
        {
            var users = await userService.GetAllUsers(request);

            return await PaginatedList<UserResponse>.CreateAsync(users.AsQueryable(), mapper, request.PageIndex, request.PageSize);
        }

    }
}
