using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using WEB.SERVICES.DTO;
using WEB.SERVICES.IService;
using WEB.SERVICES.Service.JWT;
using RefreshRequest = Microsoft.AspNetCore.Identity.Data.RefreshRequest;

namespace WEB.AUTHENTICATION.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly IUserService _userService;
        private readonly ITokenLifecycleService _tokenLifecycleService;

        public AuthController(ITokenService tokenService, IUserService userService, ITokenLifecycleService tokenLifecycleService)
        {
            _tokenService = tokenService;
            _userService = userService;
            _tokenLifecycleService = tokenLifecycleService;
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
        //public async Task<IActionResult> Login([FromBody] LoginDto dto)
        //{
        //    var user = await _userService.ValidateCredentialsAsync(dto.Username, dto.Password);
        //    if (user == null) return Unauthorized("Invalid credentials");

        //    var claims = new List<Claim>
        //    {
        //        new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
        //        new Claim(ClaimTypes.Name, user.Username),
        //        new Claim(ClaimTypes.Role, user.Role.Name)
        //    };

        //    var accessToken = _tokenService.GenerateAccessToken(claims);
        //    var jti = new JwtSecurityTokenHandler().ReadJwtToken(accessToken).Id;

        //    var tokenRecord = await _tokenLifecycleService.IssueTokenAsync(user.UserID, jti, "web");

        //    return Ok(new
        //    {
        //        AccessToken = accessToken,
        //        RefreshToken = tokenRecord.RefreshToken,
        //        TokenId = tokenRecord.TokenID,
        //        ExpiresIn = TimeSpan.FromMinutes(30).TotalSeconds
        //    });
        //}

        //[HttpPost("refresh")]
        //public async Task<IActionResult> RefreshToken([FromBody] RefreshRequestDto dto)
        //{
        //    var token = await _tokenLifecycleService.GetByRefreshTokenAsync(dto.RefreshToken);
        //    if (token == null || token.RefreshTokenExpiry < DateTime.UtcNow)
        //        return Unauthorized("Invalid or expired refresh token");

        //    var user = token.User;

        //    var claims = new List<Claim>
        //{
        //    new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
        //    new Claim(ClaimTypes.Name, user.Username),
        //    new Claim(ClaimTypes.Role, user.Role.Name)
        //};

        //    var newAccessToken = _tokenService.GenerateAccessToken(claims);
        //    var newJti = new JwtSecurityTokenHandler().ReadJwtToken(newAccessToken).Id;

        //    token.AccessTokenJti = newJti;
        //    await _tokenLifecycleService.RotateRefreshTokenAsync(token.TokenID);

        //    return Ok(new
        //    {
        //        AccessToken = newAccessToken,
        //        RefreshToken = token.RefreshToken,
        //        TokenId = token.TokenID,
        //        ExpiresIn = TimeSpan.FromMinutes(30).TotalSeconds
        //    });
        //}

        [HttpPost("revoke")]
        [Authorize]
        public async Task<IActionResult> RevokeToken([FromBody] Guid tokenId)
        {
            await _tokenLifecycleService.RevokeTokenAsync(tokenId);
            return Ok("Token revoked");
        }
    }
}
