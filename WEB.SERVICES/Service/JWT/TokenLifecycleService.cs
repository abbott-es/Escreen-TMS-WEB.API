using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
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
        private readonly IUserContextService _userContextService;

        public TokenLifecycleService(
            IRepository<UserToken> tokenRepository,
            IUnitOfWork unitOfWork,
            ITokenService tokenService,
            JwtSettings settings,
            IUserContextService userContextService)
        {
            _tokenRepository = tokenRepository;
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _settings = settings;
            _userContextService = userContextService;
        }

        public async Task<UserToken> IssueTokenAsync(Guid userId, string jti)
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
                DeviceInfo = _userContextService.DeviceInfo,
                IpAddress = _userContextService.IpAddress,
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

        public async Task<bool> RevokeByAccessAndRefreshTokenAsync(string accessToken, string refreshToken)
        {
            var isValid = await ValidateTokensAsync(accessToken, refreshToken);
            if (!isValid) return false;

            var token = await GetByRefreshTokenAsync(refreshToken);
            if (token == null) return false;

            await RevokeTokenAsync(token.TokenID);
            return true;
        }

        public string? GetJtiFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
        }

        private async Task<bool> ValidateTokensAsync(string accessToken, string refreshToken)
        {
            var jti = GetJtiFromToken(accessToken);
            if (string.IsNullOrEmpty(jti)) return false;

            var token = await GetByRefreshTokenAsync(refreshToken);
            if (token == null || token.IsRevoked) return false;

            // Ensure the access token matches the stored JTI
            if (token.AccessTokenJti != jti) return false;

            // Ensure the refresh token is not expired
            if (token.RefreshTokenExpiry < DateTime.UtcNow) return false;

            return true;
        }
    }
}
