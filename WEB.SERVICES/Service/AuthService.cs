using AutoMapper;
using Azure;
using FluentValidation;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using WEB.DOMAIN.Entity;
using WEB.DOMAIN.Interface;
using WEB.SERVICES.DTO;
using WEB.SERVICES.IService;
using WEB.UTILITY.Helper;
using WEB.UTILITY.Logger;
using WEB.UTILITY.Security.ISecurity;

namespace WEB.SERVICES.Service
{
    public class AuthService : BaseService<AuthService>, IAuthService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<Auth> _authRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<UserDto> _validator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRsaEncryptionService _rsaEncryptionService;
        private readonly ITokenService _tokenService;
        private readonly ITokenLifecycleService _tokenLifecycleService;
        private readonly IUserContextService _userContextService;
        private readonly IValidator<LogoutDto> _logoutValidator;

        public AuthService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<UserDto> validator,
            IAppLogger<AuthService> appLogger,
            IRepository<Auth> authRepository,
            IRsaEncryptionService rsaEncryptionService,
            IRepository<User> userRepository,
            IUserContextService userContextService,
            ITokenService tokenService, ITokenLifecycleService tokenLifecycleService,
            IValidator<LogoutDto> logoutValidator
            ) : base(appLogger)
        {
            _mapper = mapper;
            _validator = validator;
            _unitOfWork = unitOfWork;
            _rsaEncryptionService = rsaEncryptionService;
            _authRepository = authRepository;
            _userRepository = userRepository;
            _tokenService = tokenService;
            _tokenLifecycleService = tokenLifecycleService;
            _userContextService = userContextService;
            _logoutValidator = logoutValidator;
        }

        public async Task<Either<ApiResponse<string>, ApiResponse<Guid>>> CreateUserAsync(UserDto authDTO, CancellationToken ct = default)
        {
            return await ExecuteAndEitherAsync<string, Guid>(async ct =>
            {
                var validate = await _validator.ValidateAsync(authDTO, ct);
                if (!validate.IsValid)
                {
                    var errors = validate.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                    var errorMessage = string.Join("; ", errors.Select(e => $"{e.ErrorMessage}"));
                    _logger.LogWarning($"User creation failed validation: {errorMessage}");
                    return Prelude.Left(ApiResponse<string>
                        .Fail(errors.Select(x => x.ErrorMessage).ToList(), HttpStatusCode.Unauthorized));
                }
                var user = _mapper.Map<User>(authDTO, opts =>
                {
                    opts.Items["IgnoreAuth"] = false;
                });
                try
                {
                    await _unitOfWork.ExecuteAsync(async c =>
                    {
                        await _userRepository.AddAsync(user, c);
                    }, ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error creating user: {authDTO.Auth.Username}");
                    return Prelude.Left(ApiResponse<string>.Fail(["Internal Server Error"], HttpStatusCode.InternalServerError));
                }

                _logger.LogInformation($"User created successfully: {user.UserID}");
                return Prelude.Right(ApiResponse<Guid>.Ok(user.ID));
            }, nameof(CreateUserAsync), ct);

        }

        private async Task<User?> ValidateCredentialsAsync(string username, string encryptedPassword, CancellationToken ct = default)
        {
            try
            {
                var auth = await _authRepository
                    .Query(asNoTracking: true)
                    .Include(a => a.User)
                    .ThenInclude(x => x.Role)
                    .FirstOrDefaultAsync(a => a.Username == username && a.User.IsActive, ct);

                if (auth == null)
                {
                    _logger.LogWarning($"Authentication failed: user '{username}' not found or inactive.");
                    return null;
                }

                string decryptedPass;
                try
                {
                    decryptedPass = _rsaEncryptionService.Decrypt(auth.PasswordHash);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Unable to decrypt password for user: {username}");
                    return null;
                }

                bool isValid = Isopoh.Cryptography.Argon2.Argon2.Verify(decryptedPass, encryptedPassword);
                if (!isValid)
                {
                    _logger.LogWarning($"Authentication failed: invalid password for user '{username}'.");
                    return null;
                }

                _logger.LogInformation($"User: '{username}' authenticated successfully.");
                return auth.User;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error validating credentials for user: {username}");
                throw;
            }
        }

        private async Task UpdateLastLoginAsync(string username, CancellationToken ct = default)
        {
            try
            {
                var auth = await _authRepository
                    .Query()
                    .FirstOrDefaultAsync(a => a.Username == username, ct);

                if (auth != null)
                {
                    auth.LastLogin = DateTime.UtcNow;
                    _authRepository.Update(auth);
                    _logger.LogInformation($"Updated LastLogin for user: {username}");
                }
                else
                {
                    _logger.LogWarning($"User not found for LastLogin update: {username}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating LastLogin for user: {username}");
                throw;
            }
        }

        public async Task<Either<ApiResponse<string>, ApiResponse<SessionInfoDto>>> GetSessionInfoAsync(
            ClaimsPrincipal user,
            bool isKeepAlive = false,
            CancellationToken ct = default)
        {
            try
            {
                var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                {
                    return Prelude.Left(ApiResponse<string>
                        .Fail(["User ID not found in claims"], HttpStatusCode.Unauthorized));
                }

                var session = await _tokenLifecycleService.GetActiveSessionAsync(userId, ct);
                if (session == null || session.IsRevoked || session.RefreshTokenExpiry < DateTime.UtcNow)
                {
                    return Prelude.Left(ApiResponse<string>.Fail(["No active session"], HttpStatusCode.NotFound));
                }

                if (isKeepAlive)
                {
                    await _tokenLifecycleService.TouchSessionAsync(session.TokenID, ct);
                }

                var info = new SessionInfoDto
                {
                    TokenId = session.TokenID,
                    RefreshToken = session.RefreshToken,
                    AccessToken = _userContextService.AccessToken,
                    AccessTokenJti = session.AccessTokenJti,
                    ExpiryIn = session.RefreshTokenExpiry
                };

                return Prelude.Right(ApiResponse<SessionInfoDto>
                    .Ok(info, "Session retrieved"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal Server Error");
                return Prelude.Left(ApiResponse<string>.Fail(["Internal Server Error"], HttpStatusCode.InternalServerError));
            }
        }


        public async Task<Either<ApiResponse<string>, ApiResponse<SessionInfoDto>>> TryCreateSessionAsync(Guid tokenId, CancellationToken ct = default)
        {
            try
            {
                var session = await _tokenLifecycleService.GetByTokenIdAsync(tokenId, ct);
                if (session == null || session.IsRevoked || session.RefreshTokenExpiry < DateTime.UtcNow)
                {
                    return Prelude.Left(ApiResponse<string>.Fail(["Invalid or expired session"], HttpStatusCode.Unauthorized));
                }

                var user = session.User;
                var newToken = _tokenService.GenerateAccessToken(user);
                var newJti = new JwtSecurityTokenHandler().ReadJwtToken(newToken.accessToken).Id;

                await _tokenLifecycleService.UpdateAccessTokenJtiAsync(tokenId, newJti, ct);

                var response = new SessionInfoDto
                {
                    AccessToken = newToken.accessToken,
                    RefreshToken = session.RefreshToken,
                    TokenId = tokenId,
                    ExpiryIn = newToken.expiresIn
                };

                return Prelude.Right(ApiResponse<SessionInfoDto>.Ok(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating session for TokenID: {tokenId}");
                return Prelude.Left(ApiResponse<string>.Fail(["Internal Server Error"], HttpStatusCode.InternalServerError));
            }
        }
        public async Task<Either<ApiResponse<string>, ApiResponse<SessionInfoDto>>> TryLoginAsync(AuthDto authDto, CancellationToken ct = default)
        {
            try
            {
                var user = await ValidateCredentialsAsync(authDto.Username, authDto.Password, ct);
                if (user == null)
                    return Prelude.Left(ApiResponse<string>.Fail(["Invalid Credentials"], HttpStatusCode.NotFound));

                await UpdateLastLoginAsync(authDto.Username, ct);

                var token = _tokenService.GenerateAccessToken(user);
                if (!JwtHelper.TryExtractJti(token.accessToken, out var jti))
                {
                    return Prelude.Left(ApiResponse<string>.Fail(["Failed to parse token identifier"]));
                }

                var tokenRecord = await _tokenLifecycleService.IssueTokenAsync(user.UserID, jti, ct);

                var response = new SessionInfoDto
                {
                    AccessToken = token.accessToken,
                    RefreshToken = tokenRecord.RefreshToken,
                    TokenId = tokenRecord.TokenID,
                    ExpiryIn = token.expiresIn
                };
                return Prelude.Right(ApiResponse<SessionInfoDto>.Ok(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Login error for user: {authDto.Username}");
                return Prelude.Left(ApiResponse<string>.Fail(["Internal Server Error"], HttpStatusCode.InternalServerError));
            }
        }
        public async Task<Either<ApiResponse<string>, ApiResponse<SessionInfoDto>>> TryRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
        {
            try
            {
                var token = await _tokenLifecycleService.GetByRefreshTokenAsync(refreshToken);
                if (token == null || token.RefreshTokenExpiry < DateTime.UtcNow)
                    return Prelude.Left(ApiResponse<string>.Fail(["Invalid or expired refresh token"], HttpStatusCode.NotFound));

                var newToken = _tokenService.GenerateAccessToken(token.User);
                var newJti = new JwtSecurityTokenHandler().ReadJwtToken(newToken.accessToken).Id;

                token.AccessTokenJti = newJti;
                await _tokenLifecycleService.RotateRefreshTokenAsync(token.TokenID, ct);

                var response = new SessionInfoDto
                {
                    AccessToken = newToken.accessToken,
                    RefreshToken = token.RefreshToken,
                    TokenId = token.TokenID,
                    ExpiryIn = newToken.expiresIn
                };
                return Prelude.Right(ApiResponse<SessionInfoDto>.Ok(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error refreshing token: {refreshToken}");
                return Prelude.Left(ApiResponse<string>.Fail(["Internal Server Error"], HttpStatusCode.InternalServerError));
            }
        }
        public async Task<Either<ApiResponse<string>, ApiResponse<string>>> TryRevokeTokenAsync(Guid tokenId, CancellationToken ct = default)
        {
            try
            {
                await _tokenLifecycleService.RevokeTokenAsync(tokenId);
                return Prelude.Right(ApiResponse<string>.Ok("Revoked Successful"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error revoking token: {tokenId}");
                return Prelude.Left(ApiResponse<string>.Fail(["Internal Server Error"], HttpStatusCode.InternalServerError));
            }
        }
        public async Task<Either<ApiResponse<string>, ApiResponse<string>>> TryLogoutAsync(LogoutDto request, CancellationToken ct)
        {
            try
            {
                var validate = await _logoutValidator.ValidateAsync(request, ct);
                if (!validate.IsValid)
                {
                    var errors = validate.Errors.Select(e => $"{e.PropertyName}: {e.ErrorMessage}").ToList();
                    return Prelude.Left(ApiResponse<string>.Fail(errors));
                }

                var success = await _tokenLifecycleService.RevokeByAccessAndRefreshTokenAsync(request.AccessToken, request.RefreshToken);
                if (!success) return Prelude.Left(ApiResponse<string>.Fail(["Invalid or already revoked token"]));

                return Prelude.Right(ApiResponse<string>.Ok("Logout Successful"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Logout error for token: {request.AccessToken}");
                return Prelude.Left(ApiResponse<string>.Fail(["Internal Server Error"], HttpStatusCode.InternalServerError));
            }
        }

    }
}
