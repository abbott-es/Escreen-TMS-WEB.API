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

        /// <summary>
        /// Retrieves all users per role, optionally including related navigation properties.
        /// </summary>
        /// <param name="userRoleDto">An object that consist of roleid and array of navigation property paths to include.</param>
        /// <param name="ct">Optional cancellation token.</param>
        /// <returns>An API response containing the list of users or a not found result.</returns>
        [HttpGet("GetUserByRole")]
        public async Task<IActionResult> GetAll([FromQuery] GenericFromQueryDto userRoleDto, CancellationToken ct = default)
        {
            return await ResultMatcher.MatchResultAsync(_userService.GetAllUserByRoleAsync(userRoleDto, ct));
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
        [HttpGet("GetCurrentUserRole")]
        public async Task<IActionResult> GetActiveUserRoleAsync(CancellationToken ct = default)
        {
            return await ResultMatcher.MatchResultAsync(_userService.GetActiveUserRoleAsync(ct));
        }

        /// <summary>
        /// Retrieves all users driver.
        /// </summary>
        /// <param name="ct">Optional cancellation token.</param>
        /// <returns>An API response containing the list of users driver or a not found result.</returns>
        [HttpGet("GetAllDriver")]
        public async Task<IActionResult> GetAllDriver(CancellationToken ct = default)
        {
            return await ResultMatcher.MatchResultAsync(_userService.GetAllDriver(ct));
        }

        /// <summary>
        /// Retrieves all users helper.
        /// </summary>
        /// <param name="ct">Optional cancellation token.</param>
        /// <returns>An API response containing the list of users helper or a not found result.</returns>
        [HttpGet("GetAllHelper")]
        public async Task<IActionResult> GetAllHelper(CancellationToken ct = default)
        {
            return await ResultMatcher.MatchResultAsync(_userService.GetAllHelper(ct));
        }
    }
}
