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
        public async Task<IActionResult> GetUserByIdAsync(string userID, CancellationToken ct = default)
        {
            try
            {
                var user = await _userService.GetUserDtoByIdAsync(userID, ct);
                if (user == null)
                {
                    return ApiResponse<string>
                        .Fail(["No user found"])
                        .ToNotFoundResult();
                }

                return ApiResponse<object>
                    .Ok(user, "User retrieved")
                    .ToOkResult();
            }
            catch
            {
                return ApiResponse<string>
                    .Fail(["Internal Server Error"])
                    .ToInternalServerErrorResult();
            }
        }
    }
}
