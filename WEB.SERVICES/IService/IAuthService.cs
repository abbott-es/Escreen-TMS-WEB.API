using LanguageExt;
using System.Security.Claims;
using WEB.DOMAIN.Entity;
using WEB.SERVICES.DTO;

namespace WEB.SERVICES.IService
{
    public interface IAuthService
    {
        Task<Either<string, Guid>> CreateUserAsync(UserDto authDTO, CancellationToken ct = default);
        Task<Either<string, SessionInfoDto>> GetSessionInfoAsync(ClaimsPrincipal user, bool isKeepAlive = false, CancellationToken ct = default);
        Task<Either<string, SessionInfoDto>> TryCreateSessionAsync(Guid tokenId, CancellationToken ct = default);
        Task<Either<string, SessionInfoDto>> TryLoginAsync(AuthDto authDto, CancellationToken ct = default);
        Task<Either<string, SessionInfoDto>> TryRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
        Task<Either<string, string>> TryRevokeTokenAsync(Guid tokenId, CancellationToken ct = default);
        Task<Either<string, string>> TryLogoutAsync(LogoutDto request, CancellationToken ct = default);
    }
}
