using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using WEB.SERVICES.DTO;
using WEB.SERVICES.IService;
using WEB.SERVICES.Service;

namespace WEB.AUTHENTICATION.Controllers
{
    [Route("authentication-api/api/sso/[controller]")]
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
            var result = await _authService.CreateUserAsync(authDTO, ct);
            return result.Match<IActionResult>(
                Left: error => BadRequest(new { Error = error }),
                Right: id => CreatedAtAction(
                    nameof(CreateUserHash),   // action name
                    new { id },        // route values must match {id}
                    authDTO            // response body
                )
            );
        }

        /// <summary>
        /// Authenticates a user and returns access and refresh tokens.
        /// </summary>
        /// <param name="authDto">The login credentials.</param>
        /// <returns>200 OK with tokens, 401 Unauthorized if credentials are invalid.</returns>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthDto authDto)
        {
            var user = await _authService.ValidateCredentialsAsync(authDto.Username, authDto.Password);
            if (user == null) return Unauthorized("Invalid credentials");

            var token = _tokenService.GenerateAccessToken(user);
            var jti = new JwtSecurityTokenHandler().ReadJwtToken(token.accessToken).Id;

            var tokenRecord = await _tokenLifecycleService.IssueTokenAsync(user.UserID, jti);

            return Ok(new
            {
                AccessToken = token.accessToken,
                RefreshToken = tokenRecord.RefreshToken,
                TokenId = tokenRecord.TokenID,
                ExpiresIn = token.expiresIn
            });
        }

        /// <summary>
        /// Refreshes an expired access token using a valid refresh token.
        /// </summary>
        /// <param name="refreshToken">The refresh token request.</param>
        /// <returns>200 OK with new tokens, 401 Unauthorized if token is invalid or expired.</returns>
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] string refreshToken)
        {
            var token = await _tokenLifecycleService.GetByRefreshTokenAsync(refreshToken);
            if (token == null || token.RefreshTokenExpiry < DateTime.UtcNow)
                return Unauthorized("Invalid or expired refresh token");

            var newToken = _tokenService.GenerateAccessToken(token.User);
            var newJti = new JwtSecurityTokenHandler().ReadJwtToken(newToken.accessToken).Id;

            token.AccessTokenJti = newJti;
            await _tokenLifecycleService.RotateRefreshTokenAsync(token.TokenID);

            return Ok(new
            {
                AccessToken = newToken.accessToken,
                RefreshToken = token.RefreshToken,
                TokenId = token.TokenID,
                ExpiresIn = newToken.expiresIn
            });
        }

        /// <summary>
        /// Revokes access token by its ID.
        /// </summary>
        /// <param name="tokenId">The request containing the token ID to revoke.</param>
        /// <returns>200 OK if the token was successfully revoked.</returns>
        [HttpPost("revoke")]
        public async Task<IActionResult> RevokeToken([FromBody] Guid tokenId)
        {
            await _tokenLifecycleService.RevokeTokenAsync(tokenId);
            return Ok("Token revoked");
        }

        /// <summary>
        /// Logout by the access token and refresh token.
        /// </summary>
        /// <param name="request">The request containing the access and refresh token to logout.</param>
        /// <returns>200 OK if the token was successfully logout.</returns>
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] LogoutDto request, CancellationToken ct = default)
        {
            var validate = await _logoutValidator.ValidateAsync(request, ct);
            if (!validate.IsValid)
            {
                var errors = validate.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                return BadRequest(errors);
            }
            var success = await _tokenLifecycleService.RevokeByAccessAndRefreshTokenAsync(request.AccessToken, request.RefreshToken);
            if (!success)
                return Unauthorized("Invalid or already revoked token.");

            return Ok(new { message = "Logout successful." });
        }
    }
}
