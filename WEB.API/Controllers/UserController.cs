using Microsoft.AspNetCore.Mvc;
using WEB.SERVICES.IService;
using WEB.UTILITY.Helper;
using WEB.UTILITY.Extension;

namespace WEB.API.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]

    public class UserController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
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
        /// Retrieves the user role name.
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
