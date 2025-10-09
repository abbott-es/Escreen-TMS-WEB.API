using Microsoft.EntityFrameworkCore;
using WEB.DOMAIN.Entity;
using WEB.DOMAIN.Interface;
using WEB.SERVICES.IService;
using WEB.UTILITY.Security;

namespace WEB.SERVICES.Service.JWT
{

    public class TokenLifecycleService : ITokenLifecycleService
    {
        private readonly IRepository<UserToken> _tokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly JwtSettings _settings;

        public TokenLifecycleService(
            IRepository<UserToken> tokenRepository,
            IUnitOfWork unitOfWork,
            ITokenService tokenService,
            JwtSettings settings)
        {
            _tokenRepository = tokenRepository;
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _settings = settings;
        }

        public async Task<UserToken> IssueTokenAsync(Guid userId, string jti, string deviceInfo)
        {
            var token = new UserToken
            {
                TokenID = Guid.NewGuid(),
                UserID = userId,
                AccessTokenJti = jti,
                RefreshToken = _tokenService.GenerateRefreshToken(),
                RefreshTokenExpiry = DateTime.UtcNow.AddDays(_settings.RefreshTokenExpiryDays),
                IssuedAt = DateTime.UtcNow,
                IsRevoked = false,
                DeviceInfo = deviceInfo
            };

            await _unitOfWork.ExecuteAsync(async ct =>
            {
                await _tokenRepository.AddAsync(token, ct);
            });

            return token;
        }

        public async Task<UserToken?> RotateRefreshTokenAsync(Guid tokenId)
        {
            return await _unitOfWork.ExecuteAsync(async ct =>
            {
                var token = await _tokenRepository.GetByIdAsync(tokenId, ct);
                if (token == null || token.IsRevoked) return null;

                token.RefreshToken = _tokenService.GenerateRefreshToken();
                token.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_settings.RefreshTokenExpiryDays);
                _tokenRepository.Update(token);
                return token;
            });
        }

        public async Task RevokeTokenAsync(Guid tokenId)
        {
            await _unitOfWork.ExecuteAsync(async ct =>
            {
                var token = await _tokenRepository.GetByIdAsync(tokenId, ct);
                if (token == null) return;

                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
                _tokenRepository.Update(token);
            });
        }

        public async Task<bool> IsAccessTokenRevokedAsync(string jti)
        {
            return await _unitOfWork.ExecuteReadOnlyAsync(async ct =>
            {
                var tokens = await _tokenRepository.GetAllAsync(ct);
                return tokens.Any(t => t.AccessTokenJti == jti && t.IsRevoked);
            });
        }

        public async Task<UserToken?> GetByRefreshTokenAsync(string refreshToken)
        {
            return await _unitOfWork.ExecuteReadOnlyAsync(async ct =>
            {
                return await _tokenRepository
                    .Query(asNoTracking: true)
                    .Include(t => t.User)
                    .ThenInclude(u => u.Role)
                    .Include(a => a.User)
                    .ThenInclude(a => a.Auth)
                    .FirstOrDefaultAsync(t => t.RefreshToken == refreshToken && !t.IsRevoked, ct);
            });
        }

        public async Task<UserToken?> GetActiveSessionAsync(Guid userId)
        {
            return await _unitOfWork.ExecuteReadOnlyAsync(async ct =>
            {
                return await _tokenRepository
                    .Query(asNoTracking: true)
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.UserID == userId && !t.IsRevoked && t.RefreshTokenExpiry > DateTime.UtcNow, ct);
            });
        }

        public async Task UpdateAccessTokenJtiAsync(Guid tokenId, string newJti)
        {
            await _unitOfWork.ExecuteAsync(async ct =>
            {
                var token = await _tokenRepository.GetByIdAsync(tokenId, ct);
                if (token == null || token.IsRevoked) return;

                token.AccessTokenJti = newJti;
                _tokenRepository.Update(token);
            });
        }
        public async Task<UserToken?> GetByTokenIdAsync(Guid tokenId)
        {
            return await _unitOfWork.ExecuteReadOnlyAsync(async ct =>
            {
                return await _tokenRepository
                    .Query(asNoTracking: true)
                    .Include(t => t.User)
                    .ThenInclude(u => u.Role)
                    .FirstOrDefaultAsync(t => t.TokenID == tokenId, ct);
            });
        }
    }
}
