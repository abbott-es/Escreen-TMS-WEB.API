using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WEB.SERVICES.IService;

namespace WEB.AUTHENTICATION.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class SessionController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly ITokenLifecycleService _tokenLifecycleService;
        private readonly IUserService _userService;

        public SessionController(ITokenService tokenService, ITokenLifecycleService tokenLifecycleService, IUserService userService)
        {
            _tokenService = tokenService;
            _tokenLifecycleService = tokenLifecycleService;
            _userService = userService;
        }

        /// <summary>
        /// Retrieves the current active session for the authenticated user.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <remarks>
        /// This endpoint returns the session details such as token ID, refresh token expiry, and access token JTI.
        /// It requires the user to be authenticated.
        /// </remarks>
        /// <returns>
        /// 200 OK with session details if an active session exists.
        /// 404 Not Found if no active session is found.
        /// </returns>
        [HttpGet]
        public async Task<IActionResult> Session(CancellationToken ct = default)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var session = await _tokenLifecycleService.GetActiveSessionAsync(Guid.Parse(userId), ct);

                if (session == null)
                {
                    return NotFound("No active session");
                }

                return Ok(new
                {
                    session.TokenID,
                    session.RefreshTokenExpiry,
                    session.AccessTokenJti
                });
            }
            catch
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

        /// <summary>
        /// Renews the access token for the authenticated user's active session.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <remarks>
        /// This endpoint is used to keep the session alive by issuing a new access token without rotating the refresh token.
        /// It requires the user to be authenticated and have an active session.
        /// </remarks>
        /// <returns>
        /// 200 OK with a new access token if the session is valid.
        /// 401 Unauthorized if no active session is found.
        /// </returns>
        [HttpPost("keep-alive")]
        public async Task<IActionResult> KeepAlive(CancellationToken ct = default)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _userService.GetUserByIdAsync(userId);
                var session = await _tokenLifecycleService.GetActiveSessionAsync(Guid.Parse(userId), ct);

                if (session == null)
                    return Unauthorized("No active session");

                var newToken = _tokenService.GenerateAccessToken(user);
                var newJti = new JwtSecurityTokenHandler().ReadJwtToken(newToken.accessToken).Id;

                await _tokenLifecycleService.UpdateAccessTokenJtiAsync(session.TokenID, newJti);

                return Ok(new
                {
                    AccessToken = newToken.accessToken,
                    TokenId = session.TokenID,
                    ExpiresIn = newToken.expiresIn
                });
            }
            catch
            {
                return StatusCode(500, "Internal Server Error");
            }
        }

        /// <summary>
        /// Creates a new access token using a valid token session.
        /// </summary>
        /// <param name="tokenId">The ID of the token session.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <remarks>
        /// This endpoint is used to restore a session by issuing a new access token using a valid, non-revoked token.
        /// </remarks>
        /// <returns>
        /// 200 OK with a new access token if the session is valid.
        /// 401 Unauthorized if the session is invalid, revoked, or expired.
        /// </returns>
        [HttpPost("create-session")]
        public async Task<IActionResult> CreateSession([FromBody] Guid tokenId, CancellationToken ct = default)
        {
            try
            {
                var session = await _tokenLifecycleService.GetByTokenIdAsync(tokenId, ct);
                if (session == null || session.IsRevoked || session.RefreshTokenExpiry < DateTime.UtcNow)
                    return Unauthorized("Invalid or expired session");

                var user = session.User;
                var newToken = _tokenService.GenerateAccessToken(user);
                var newJti = new JwtSecurityTokenHandler().ReadJwtToken(newToken.accessToken).Id;

                await _tokenLifecycleService.UpdateAccessTokenJtiAsync(tokenId, newJti, ct);

                return Ok(new
                {
                    AccessToken = newToken.accessToken,
                    TokenId = tokenId,
                    ExpiresIn = newToken.expiresIn
                });
            }
            catch
            {
                return StatusCode(500, "Internal Server Error");
            }
        }
    }
}
