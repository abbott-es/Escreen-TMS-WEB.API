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
        Task<UserToken> IssueTokenAsync(Guid userId, string jti, string deviceInfo);
        Task<UserToken?> RotateRefreshTokenAsync(Guid tokenId);
        Task RevokeTokenAsync(Guid tokenId);
        Task<bool> IsAccessTokenRevokedAsync(string jti);
        Task<UserToken> GetByRefreshTokenAsync(string refreshToken);
    }

}
