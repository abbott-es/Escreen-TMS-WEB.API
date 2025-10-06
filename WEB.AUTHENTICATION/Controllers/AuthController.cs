using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WEB.AUTHENTICATION.JWT;
using WEB.SERVICES.DTO;
using WEB.SERVICES.IService;
using RefreshRequest = Microsoft.AspNetCore.Identity.Data.RefreshRequest;

namespace WEB.AUTHENTICATION.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private static readonly Dictionary<string, string> _refreshTokens = new(); // Replace with DB or Redis
        private readonly ITokenService _tokenService;
        private readonly IUserService _userService;

        public AuthController(ITokenService tokenService, IUserService userService)
        {
            _tokenService = tokenService;
            _userService = userService;
        }

        [HttpPost("create-auth")]
        public async Task<IActionResult> CreateUserHash([FromBody] UserDto authDTO, CancellationToken ct)
        {
            var result = await _userService.CreateUserAsync(authDTO, ct);
            return result.Match<IActionResult>(
                Left: error => BadRequest(new { Error = error }),
                Right: id => CreatedAtAction(
                    nameof(CreateUserHash),   // action name
                    new { id },        // route values must match {id}
                    authDTO            // response body
                )
            );
        }
        //[HttpPost("login")]
        //public IActionResult Login([FromBody] AuthDTO request)
        //{
        //    if (request.Username != "admin" || request.Password != "password")
        //        return Unauthorized();

        //    var claims = new[]
        //    {
        //        new Claim(ClaimTypes.NameIdentifier, "admin-id"),
        //        new Claim(ClaimTypes.Name, request.Username),
        //        new Claim(ClaimTypes.Role, "Admin")
        //    };

        //    var accessToken = _tokenService.GenerateAccessToken(claims);
        //    var refreshToken = _tokenService.GenerateRefreshToken();

        //    _refreshTokens[refreshToken] = "admin-id"; // Store securely

        //    return Ok(new { accessToken, refreshToken });
        //}

        //[HttpPost("refresh")]
        //public IActionResult Refresh([FromBody] WEB.SERVICES.DTO.RefreshRequest request)
        //{
        //    if (!_refreshTokens.TryGetValue(request.RefreshToken, out var userId))
        //        return Unauthorized();

        //    var claims = new[]
        //    {
        //        new Claim(ClaimTypes.NameIdentifier, userId),
        //        new Claim(ClaimTypes.Name, "admin"),
        //        new Claim(ClaimTypes.Role, "Admin")
        //    };

        //    var newAccessToken = _tokenService.GenerateAccessToken(claims);
        //    var newRefreshToken = _tokenService.GenerateRefreshToken();

        //    _refreshTokens.Remove(request.RefreshToken);
        //    _refreshTokens[newRefreshToken] = userId;

        //    return Ok(new { accessToken = newAccessToken, refreshToken = newRefreshToken });
        //}
    }
}
