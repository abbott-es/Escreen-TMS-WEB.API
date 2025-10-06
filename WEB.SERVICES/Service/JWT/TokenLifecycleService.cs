using Microsoft.EntityFrameworkCore;
using WEB.DOMAIN.Entity;
using WEB.DOMAIN.Interface;
using WEB.SERVICES.IService;

namespace WEB.SERVICES.Service.JWT
{

    public class TokenLifecycleService : ITokenLifecycleService
    {
        private readonly IRepository<UserToken> _tokenRepository;
        private readonly IEFUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;

        public TokenLifecycleService(
            IRepository<UserToken> tokenRepository,
            IEFUnitOfWork unitOfWork,
            ITokenService tokenService)
        {
            _tokenRepository = tokenRepository;
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
        }

        public async Task<UserToken> IssueTokenAsync(Guid userId, string jti, string deviceInfo)
        {
            var token = new UserToken
            {
                TokenID = Guid.NewGuid(),
                UserID = userId,
                AccessTokenJti = jti,
                RefreshToken = _tokenService.GenerateRefreshToken(),
                RefreshTokenExpiry = DateTime.UtcNow.AddDays(7),
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
                token.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
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
                    .FirstOrDefaultAsync(t => t.RefreshToken == refreshToken && !t.IsRevoked, ct);
            });
        }

    }


}
