using Microsoft.AspNetCore.Mvc;
using WEB.SERVICES.DTO;
using WEB.SERVICES.IService;
using WEB.UTILITY.Helper;

namespace WEB.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]

    public class UserController : GenericController<UserDto>
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService, IGenericService<UserDto> genericService) : base(genericService)
        {
            _userService = userService;
        }

        [NonAction]
        public override async Task<IActionResult> GetAll([FromQuery] string[] includes, CancellationToken ct = default)
        {
            return await base.GetAll(null,ct);
        }

        [NonAction]
        public override async Task<IActionResult> Update([FromBody] UserDto entity, CancellationToken ct = default)
        {
            return await base.Update(null, ct);
        }
        /// <summary>
        /// Retrieves all users per role, optionally including related navigation properties.
        /// </summary>
        /// <param name="userRoleDto">An object thhat consist of roleid and array of navigation property paths to include.</param>
        /// <param name="ct">Optional cancellation token.</param>
        /// <returns>An API response containing the list of users or a not found result.</returns>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] UserRoleDto userRoleDto, CancellationToken ct = default)
        {
            return await ResultMatcher.MatchResultAsync(_userService.GetAllUserByRoleAsync(userRoleDto,ct));
        }

        /// <summary>
        /// Updates an existing user details.
        /// </summary>
        /// <param name="entity">The user detail with updated data.</param>
        /// <param name="ct">Optional cancellation token.</param>
        /// <returns>An API response indicating success or failure.</returns>
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateUserDto entity, CancellationToken ct = default)
        {
            return await ResultMatcher.MatchResultAsync(_userService.UpdateUserAsync(entity, ct));
        }

        /// <summary>
        /// Retrieves the user details.
        /// </summary>
        /// <param name="userID">User ID.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <remarks>
        /// This endpoint returns the user details such as role ID, and user information.
        /// It requires the user to be authenticated.
        /// </remarks>
        /// <returns>
        /// 200 OK with user details.
        /// 404 Not Found if no user is found.
        /// </returns>
        [HttpGet("GetUserByID/{userID}")]
        public async Task<IActionResult> GetUserByIdAsync(Guid userID, CancellationToken ct = default)
        {
            return await ResultMatcher.MatchResultAsync(_userService.GetUserDtoByIdAsync(userID, ct));
        }

        /// <summary>
        /// Retrieves the authenticated user role name.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <remarks>
        /// This endpoint returns the user role.
        /// It requires the user to be authenticated.
        /// </remarks>
        /// <returns>
        /// 200 OK with user role.
        /// 404 Not Found if no user role is found.
        /// </returns>
        [HttpGet("GetUserRole")]
        public async Task<IActionResult> GetActiveUserRoleAsync(CancellationToken ct = default)
        {
            return await ResultMatcher.MatchResultAsync(_userService.GetActiveUserRoleAsync(ct));
        }
    }
}
