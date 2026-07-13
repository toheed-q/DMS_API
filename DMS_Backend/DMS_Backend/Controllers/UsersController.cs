using DMS_Backend.Common;
using DMS_Backend.Contracts;
using DMS_Backend.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DMS_Backend.Controllers
{
    /// <summary>User administration — admins only.</summary>
    [Authorize(Roles = Roles.Admin)]
    [Route("api/users")]
    public class UsersController : BaseApiController
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService) => _userService = userService;

        /// <summary>Creates a login. Use role "Salesman" + salesmanId for a mobile user.</summary>
        [HttpPost]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] CreateUserRequest request)
            => HandleCreated(await _userService.CreateAsync(request));

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] PaginationQuery query)
            => HandleResult(await _userService.GetUsersAsync(query));
    }
}
