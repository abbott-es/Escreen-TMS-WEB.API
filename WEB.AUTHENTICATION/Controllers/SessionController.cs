using Microsoft.AspNetCore.Mvc;
using WEB.SERVICES.DTO;
using WEB.SERVICES.IService;
using WEB.UTILITY.Helper;

namespace WEB.AUTHENTICATION.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class SessionController : ControllerBase
    {
        private readonly IAuthService _authService;
        public SessionController(IAuthService authService)
        {
            _authService = authService;
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
            return await ResultMatcher.MatchResultAsync(_authService.GetSessionInfoAsync(User, false, ct));
        }

        /// <summary>
        /// Authenticated user's active session.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <remarks>
        /// This endpoint is used to see the current session alive by without rotating the refresh token and access token.
        /// It requires the user to be authenticated and have an active session.
        /// </remarks>
        /// <returns>
        /// 200 OK with a session if the session is valid.
        /// 401 Unauthorized if no active session is found.
        /// </returns>
        [HttpPost("keep-alive")]
        public async Task<IActionResult> KeepAlive(CancellationToken ct = default)
        {
            return await ResultMatcher.MatchResultAsync(_authService.GetSessionInfoAsync(User, true, ct));
        }

        /// <summary>
        /// Creates a new access token using a valid token session.
        /// </summary>
        /// <param name="accessTokenJti">The ID of the access token session.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <remarks>
        /// This endpoint is used to restore a session by issuing a new access token using a valid, non-revoked token.
        /// </remarks>
        /// <returns>
        /// 200 OK with a new access token if the session is valid.
        /// 401 Unauthorized if the session is invalid, revoked, or expired.
        /// </returns>

        [HttpPost("create-session")]
        public async Task<IActionResult> CreateSession([FromBody] GenericFieldDto genericFieldDto, CancellationToken ct = default)
        {
            return await ResultMatcher.MatchResultAsync(_authService.TryCreateSessionAsync(genericFieldDto.ID, ct));
        }
    }
}
