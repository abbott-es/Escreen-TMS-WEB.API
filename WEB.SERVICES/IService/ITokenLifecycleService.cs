using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WEB.DOMAIN.Entity;

namespace WEB.SERVICES.IService
{
    public interface ITokenLifecycleService
    {
        Task<UserToken> IssueTokenAsync(Guid userId, string jti, CancellationToken ct = default);
        Task<UserToken?> RotateRefreshTokenAsync(Guid tokenId, string jti, CancellationToken ct = default);
        Task RevokeTokenAsync(Guid tokenId, CancellationToken ct = default);
        Task<bool> IsAccessTokenRevokedAsync(string jti, CancellationToken ct = default);
        Task<UserToken> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
        Task<UserToken?> GetActiveSessionAsync(Guid userId, CancellationToken ct = default);
        Task UpdateAccessTokenJtiAsync(Guid tokenId, string newJti, CancellationToken ct = default);
        Task<UserToken?> GetByTokenIdAsync(Guid tokenId, CancellationToken ct = default);
        Task<bool> RevokeByAccessAndRefreshTokenAsync(string accessToken, string refreshToken, CancellationToken ct = default);
        string? GetJtiFromToken(string token);
        Task TouchSessionAsync(Guid tokenId, CancellationToken ct = default);
    }

}
