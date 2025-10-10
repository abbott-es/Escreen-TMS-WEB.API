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
        Task<UserToken> IssueTokenAsync(Guid userId, string jti);
        Task<UserToken?> RotateRefreshTokenAsync(Guid tokenId);
        Task RevokeTokenAsync(Guid tokenId);
        Task<bool> IsAccessTokenRevokedAsync(string jti);
        Task<UserToken> GetByRefreshTokenAsync(string refreshToken);
        Task<UserToken?> GetActiveSessionAsync(Guid userId);
        Task UpdateAccessTokenJtiAsync(Guid tokenId, string newJti);
        Task<UserToken?> GetByTokenIdAsync(Guid tokenId);
        Task<bool> RevokeByAccessAndRefreshTokenAsync(string accessToken, string refreshToken);
        string? GetJtiFromToken(string token);
    }

}
