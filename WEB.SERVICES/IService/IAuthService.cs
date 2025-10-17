using LanguageExt;
using System.Security.Claims;
using WEB.DOMAIN.Entity;
using WEB.SERVICES.DTO;
using WEB.UTILITY.Helper;

namespace WEB.SERVICES.IService
{
    public interface IAuthService
    {
        Task<Either<ApiResponse<string>, ApiResponse<Guid>>> CreateUserAsync(UserDto authDTO, CancellationToken ct = default);
        Task<Either<ApiResponse<string>, ApiResponse<SessionInfoDto>>> GetSessionInfoAsync(
            ClaimsPrincipal user,
            bool isKeepAlive = false,
            CancellationToken ct = default);
        Task<Either<ApiResponse<string>, ApiResponse<SessionInfoDto>>> TryCreateSessionAsync(Guid jti, CancellationToken ct = default);
        Task<Either<ApiResponse<string>, ApiResponse<SessionInfoDto>>> TryLoginAsync(AuthDto authDto, CancellationToken ct = default);
        Task<Either<ApiResponse<string>, ApiResponse<SessionInfoDto>>> TryRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
        Task<Either<ApiResponse<string>, ApiResponse<string>>> TryRevokeTokenAsync(Guid tokenId, CancellationToken ct = default);
        Task<Either<ApiResponse<string>, ApiResponse<string>>> TryLogoutAsync(LogoutDto request, CancellationToken ct = default);
        Task<Either<ApiResponse<string>, ApiResponse<string>>> TryValidateAccessToken(CancellationToken ct);
    }
}
