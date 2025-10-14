using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using WEB.SERVICES.DTO;
using WEB.SERVICES.IService;
using WEB.UTILITY.Helper;
using WEB.UTILITY.Extension;

namespace WEB.AUTHENTICATION.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthenticationController(
            IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Creates a new user with hashed credentials.
        /// </summary>
        /// <param name="authDTO">The user data including auth and user info.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>201 Created if successful, 400 Bad Request if validation fails.</returns>
        [HttpPost("create-auth")]
        public async Task<IActionResult> CreateUserHash([FromBody] UserDto authDTO, CancellationToken ct = default)
        {
            try
            {
                var result = await _authService.CreateUserAsync(authDTO, ct);
                return result.Match<IActionResult>(
                    Left: error => ApiResponse<string>
                        .Fail([error], "User creation failed")
                        .ToBadRequestResult(),

                    Right: id => ApiResponse<UserDto>
                        .Ok(authDTO, "User created")
                        .ToCreatedResult()
                );
            }
            catch
            {
                return ApiResponse<string>
                    .Fail(["Internal Server Error"])
                    .ToInternalServerErrorResult();
            }
        }

        /// <summary>
        /// Authenticates a user and returns access and refresh tokens.
        /// </summary>
        /// <param name="authDto">The login credentials.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>200 OK with tokens, 401 Unauthorized if credentials are invalid.</returns>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthDto authDto, CancellationToken ct = default)
        {
            var result = await _authService.TryLoginAsync(authDto, ct);
            return result.Match(
                Left: error => ApiResponse<string>
                    .Fail([error])
                    .ToUnauthorizedResult(),

                Right: response => ApiResponse<object>
                    .Ok(response, "Login successful")
                    .ToOkResult()
            );
        }

        /// <summary>
        /// Refreshes an expired access token using a valid refresh token.
        /// </summary>
        /// <param name="refreshToken">The refresh token request.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>200 OK with new tokens, 401 Unauthorized if token is invalid or expired.</returns>
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken, CancellationToken ct = default)
        {
            var result = await _authService.TryRefreshTokenAsync(refreshToken, ct);

            return result.Match(
                Left: error => ApiResponse<string>
                    .Fail([error])
                    .ToUnauthorizedResult(),

                Right: response => ApiResponse<object>
                    .Ok(response, "Token refreshed")
                    .ToOkResult()
            );
        }

        /// <summary>
        /// Revokes access token by its ID.
        /// </summary>
        /// <param name="tokenId">The request containing the token ID to revoke.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>200 OK if the token was successfully revoked.</returns>
        [HttpPost("revoke")]
        public async Task<IActionResult> RevokeToken([FromBody] Guid tokenId, CancellationToken ct = default)
        {
            var result = await _authService.TryRevokeTokenAsync(tokenId, ct);

            return result.Match(
                Left: error => ApiResponse<string>
                    .Fail([error])
                    .ToInternalServerErrorResult(),

                Right: _ => ApiResponse<string>
                    .Ok(string.Empty, "Token revoked")
                    .ToOkResult()
            );
        }

        /// <summary>
        /// Logout by the access token and refresh token.
        /// </summary>
        /// <param name="request">The request containing the access and refresh token to logout.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>200 OK if the token was successfully logout.</returns>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutDto request, CancellationToken ct = default)
        {
            var result = await _authService.TryLogoutAsync(request, ct);

            return result.Match(
                Left: error =>
                {
                    var isValidationError = error.Contains(":");
                    return isValidationError
                        ? ApiResponse<string>.Fail([error], "Validation failed").ToBadRequestResult()
                        : ApiResponse<string>.Fail([error]).ToUnauthorizedResult();
                },
                Right: _ => ApiResponse<string>
                    .Ok(string.Empty, "Logout successful")
                    .ToOkResult()
            );
        }
    }
}
