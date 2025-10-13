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
        private readonly ITokenService _tokenService;
        private readonly IAuthService _authService;
        private readonly ITokenLifecycleService _tokenLifecycleService;
        private readonly IValidator<LogoutDto> _logoutValidator;

        public AuthenticationController(ITokenService tokenService,
            IAuthService authService,
            ITokenLifecycleService tokenLifecycleService,
            IValidator<LogoutDto> logoutValidator)
        {
            _tokenService = tokenService;
            _authService = authService;
            _tokenLifecycleService = tokenLifecycleService;
            _logoutValidator = logoutValidator;
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
            try
            {
                var user = await _authService.ValidateCredentialsAsync(authDto.Username, authDto.Password, ct);
                if (user == null)
                    return ApiResponse<string>
                        .Fail(["Invalid credentials"])
                        .ToUnauthorizedResult();
                // Log LastLogin timestamp
                await _authService.UpdateLastLoginAsync(authDto.Username, ct);
                var token = _tokenService.GenerateAccessToken(user);

                if (!JwtHelper.TryExtractJti(token.accessToken, out var jti))
                    return ApiResponse<string>
                        .Fail(["Failed to parse token identifier"])
                        .ToInternalServerErrorResult();

                var tokenRecord = await _tokenLifecycleService.IssueTokenAsync(user.UserID, jti, ct);

                var response = new
                {
                    AccessToken = token.accessToken,
                    RefreshToken = tokenRecord.RefreshToken,
                    TokenId = tokenRecord.TokenID,
                    ExpiresIn = token.expiresIn
                };

                return ApiResponse<object>
                    .Ok(response, "Login successful")
                    .ToOkResult();
            }
            catch
            {
                return ApiResponse<string>
                    .Fail(["Internal Server Error"])
                    .ToInternalServerErrorResult();
            }
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
            try
            {
                var token = await _tokenLifecycleService.GetByRefreshTokenAsync(refreshToken);
                if (token == null || token.RefreshTokenExpiry < DateTime.UtcNow)
                    return ApiResponse<string>
                        .Fail(["Invalid or expired refresh token"])
                        .ToUnauthorizedResult();

                var newToken = _tokenService.GenerateAccessToken(token.User);
                var newJti = new JwtSecurityTokenHandler().ReadJwtToken(newToken.accessToken).Id;

                token.AccessTokenJti = newJti;
                await _tokenLifecycleService.RotateRefreshTokenAsync(token.TokenID, ct);

                var response = new
                {
                    AccessToken = newToken.accessToken,
                    RefreshToken = token.RefreshToken,
                    TokenId = token.TokenID,
                    ExpiresIn = newToken.expiresIn
                };

                return ApiResponse<object>
                    .Ok(response, "Token refreshed")
                    .ToOkResult();
            }
            catch
            {
                return ApiResponse<string>
                    .Fail(["Internal Server Error"])
                    .ToInternalServerErrorResult();
            }
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
            try
            {
                await _tokenLifecycleService.RevokeTokenAsync(tokenId);
                return ApiResponse<string>
                    .Ok(string.Empty,"Token revoked")
                    .ToOkResult();
            }
            catch
            {
                return ApiResponse<string>
                    .Fail(["Internal Server Error"])
                    .ToInternalServerErrorResult();
            }
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
            try
            {
                var validate = await _logoutValidator.ValidateAsync(request, ct);
                if (!validate.IsValid)
                {
                    var errors = validate.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}").ToList();
                    return ApiResponse<string>
                        .Fail(errors, "Validation failed")
                        .ToBadRequestResult();
                }

                var success = await _tokenLifecycleService.RevokeByAccessAndRefreshTokenAsync(request.AccessToken, request.RefreshToken);
                if (!success)
                    return ApiResponse<string>
                        .Fail(["Invalid or already revoked token"])
                        .ToUnauthorizedResult();

                return ApiResponse<string>
                    .Ok(string.Empty,"Logout successful")
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
