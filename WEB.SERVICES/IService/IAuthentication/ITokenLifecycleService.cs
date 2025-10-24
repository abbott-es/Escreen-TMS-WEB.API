using WEB.DOMAIN.Entity.Authentication;
using WEB.DOMAIN.Entity.Generic;

namespace WEB.SERVICES.IService.IAuthentication
{
    public interface ITokenLifecycleService
    {
        Task<UserToken> IssueTokenAsync(User user, string jti, CancellationToken ct = default);
        Task<UserToken?> RotateRefreshTokenAsync(Guid tokenId, string jti, CancellationToken ct = default);
        Task RevokeTokenAsync(Guid tokenId, CancellationToken ct = default);
        Task<bool> IsAccessTokenRevokedAsync(string jti, CancellationToken ct = default);
        Task<UserToken> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
        Task<UserToken?> GetActiveSessionAsync(string jti, CancellationToken ct = default);
        Task UpdateAccessTokenJtiAsync(Guid tokenId, string newJti, CancellationToken ct = default);
        Task<UserToken?> GetByTokenIdAsync(Guid tokenId, CancellationToken ct = default);
        Task<bool> RevokeByAccessAndRefreshTokenAsync(string accessToken, string refreshToken, CancellationToken ct = default);
        string? GetJtiFromToken(string token);
        Task TouchSessionAsync(Guid tokenId, CancellationToken ct = default);
        Task<bool> IsActiveLogin(Guid userID, CancellationToken ct = default);
    }
}
