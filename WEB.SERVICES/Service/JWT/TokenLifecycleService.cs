using LanguageExt;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using WEB.DAL.Repository;
using WEB.DOMAIN.Entity;
using WEB.DOMAIN.Interface;
using WEB.SERVICES.DTO;
using WEB.SERVICES.IService;
using WEB.UTILITY.Logger;
using WEB.UTILITY.Security;

namespace WEB.SERVICES.Service.JWT
{

    public class TokenLifecycleService : BaseService<TokenLifecycleService>, ITokenLifecycleService
    {
        private readonly IRepository<UserToken> _tokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITokenService _tokenService;
        private readonly JwtSettings _settings;
        private readonly IUserContextService _userContextService;
        private readonly IAppLogger<TokenLifecycleService> _appLogger;

        public TokenLifecycleService(
            IRepository<UserToken> tokenRepository,
            IUnitOfWork unitOfWork,
            ITokenService tokenService,
            JwtSettings settings,
            IUserContextService userContextService,
            IAppLogger<TokenLifecycleService> appLogger) : base(appLogger)
        {
            _tokenRepository = tokenRepository;
            _unitOfWork = unitOfWork;
            _tokenService = tokenService;
            _settings = settings;
            _userContextService = userContextService;
        }

        public async Task<UserToken> IssueTokenAsync(Guid userId, string jti, CancellationToken ct = default)
        {
            return await ExecuteWithLoggingAsync(async ct =>
            {
                try
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
                    }, ct);

                    return token;
                }
                catch (Exception err)
                {
                    _appLogger.LogError(err, $"Error adding token for userId: {userId}");
                    throw;
                }
            }, nameof(IssueTokenAsync), ct);
        }

        public async Task<UserToken?> RotateRefreshTokenAsync(Guid tokenId, CancellationToken ct = default)
        {
            try
            {
                return await _unitOfWork.ExecuteAsync(async ct =>
                {
                    var token = await _tokenRepository.GetByIdAsync(tokenId, ct);
                    if (token == null || token.IsRevoked)
                    {
                        _logger.LogWarning($"Cannot rotate token: TokenID {tokenId} not found or revoked.");
                        return null;
                    }

                    token.RefreshToken = _tokenService.GenerateRefreshToken();
                    token.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_settings.RefreshTokenExpiryDays);
                    _tokenRepository.Update(token);
                    return token;
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error rotating refresh token for TokenID: {tokenId}");
                throw;
            }
        }

        public async Task RevokeTokenAsync(Guid tokenId, CancellationToken ct = default)
        {
            try
            {
                await _unitOfWork.ExecuteAsync(async ct =>
                {
                    var token = await _tokenRepository.GetByIdAsync(tokenId, ct);
                    if (token == null)
                    {
                        _logger.LogWarning($"Token not found for revocation: TokenID {tokenId}");
                        return;
                    }

                    token.IsRevoked = true;
                    token.RevokedAt = DateTime.UtcNow;
                    _tokenRepository.Update(token);
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error revoking token: TokenID {tokenId}");
                throw;
            }
        }

        public async Task<bool> IsAccessTokenRevokedAsync(string jti, CancellationToken ct = default)
        {
            try
            {
                return await _unitOfWork.ExecuteReadOnlyAsync(async ct =>
                {
                    var tokens = await _tokenRepository.GetAllAsync(ct);
                    return tokens.Any(t => t.AccessTokenJti == jti && t.IsRevoked);
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking revocation status for JTI: {jti}");
                throw;
            }
        }

        public async Task<UserToken?> GetByRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
        {
            try
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
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving token by refresh token: {refreshToken}");
                throw;
            }
        }

        public async Task<UserToken?> GetActiveSessionAsync(Guid userId, CancellationToken ct = default)
        {
            try
            {
                return await _unitOfWork.ExecuteReadOnlyAsync(async ct =>
                {
                    return await _tokenRepository
                        .Query(asNoTracking: true)
                        .Include(t => t.User)
                        .FirstOrDefaultAsync(t => t.UserID == userId && !t.IsRevoked && t.RefreshTokenExpiry > DateTime.UtcNow, ct);
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving active session for UserID: {userId}");
                throw;
            }
        }

        public async Task UpdateAccessTokenJtiAsync(Guid tokenId, string newJti, CancellationToken ct = default)
        {
            try
            {
                await _unitOfWork.ExecuteAsync(async ct =>
                {
                    var token = await _tokenRepository.GetByIdAsync(tokenId, ct);
                    if (token == null || token.IsRevoked)
                    {
                        _logger.LogWarning($"Cannot update JTI: TokenID {tokenId} not found or revoked.");
                        return;
                    }

                    token.AccessTokenJti = newJti;
                    _tokenRepository.Update(token);
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating JTI for TokenID: {tokenId}");
                throw;
            }
        }

        public async Task<UserToken?> GetByTokenIdAsync(Guid tokenId, CancellationToken ct = default)
        {
            try
            {
                return await _unitOfWork.ExecuteReadOnlyAsync(async ct =>
                {
                    return await _tokenRepository
                        .Query(asNoTracking: true)
                        .Include(t => t.User)
                            .ThenInclude(u => u.Role)
                        .Include(t => t.User)
                            .ThenInclude(u => u.Auth)
                        .FirstOrDefaultAsync(t => t.TokenID == tokenId, ct);
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving token by TokenID: {tokenId}");
                throw;
            }
        }

        public async Task<bool> RevokeByAccessAndRefreshTokenAsync(string accessToken, string refreshToken, CancellationToken ct = default)
        {
            try
            {
                var isValid = await ValidateTokensAsync(accessToken, refreshToken, ct);
                if (!isValid)
                {
                    _logger.LogWarning("Token validation failed during revocation.");
                    return false;
                }

                var token = await GetByRefreshTokenAsync(refreshToken, ct);
                if (token == null)
                {
                    _logger.LogWarning("Refresh token not found during revocation.");
                    return false;
                }

                await RevokeTokenAsync(token.TokenID, ct);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking token by access and refresh token.");
                throw;
            }
        }

        public string? GetJtiFromToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                return jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
            }
            catch (Exception ex)
            {
                _appLogger.LogError(ex, "Failed to extract JTI from access token.");
                return null;
            }
        }

        private async Task<bool> ValidateTokensAsync(string accessToken, string refreshToken, CancellationToken ct = default)
        {
            try
            {
                var jti = GetJtiFromToken(accessToken);
                if (string.IsNullOrEmpty(jti))
                {
                    _appLogger.LogWarning("Access token missing JTI.");
                    return false;
                }

                var token = await GetByRefreshTokenAsync(refreshToken, ct);
                if (token == null)
                {
                    _appLogger.LogWarning("Refresh token not found or revoked.");
                    return false;
                }
                
                // Ensure the access token matches the stored JTI
                if (token.AccessTokenJti != jti)
                {
                    _appLogger.LogWarning("Access token JTI mismatch.");
                    return false;
                }
                
                // Ensure the refresh token is not expired
                if (token.RefreshTokenExpiry < DateTime.UtcNow)
                {
                    _appLogger.LogWarning("Refresh token expired.");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _appLogger.LogError(ex, "Error validating access and refresh tokens.");
                return false;
            }
        }

        public async Task TouchSessionAsync(Guid tokenId, CancellationToken ct = default)
        {
            try
            {
                await _unitOfWork.ExecuteAsync(async ct =>
                {
                    var session = await _tokenRepository.GetByIdAsync(tokenId, ct);
                    if (session == null || session.IsRevoked)
                    {
                        _logger.LogWarning($"Cannot touch session: TokenID {tokenId} not found or revoked.");
                        return;
                    }
                    session.LastAccessedUtc = DateTime.UtcNow;
                    _tokenRepository.Update(session);
                }, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error touching session for TokenID: {tokenId}");
                throw;
            }
        }
    }
}
