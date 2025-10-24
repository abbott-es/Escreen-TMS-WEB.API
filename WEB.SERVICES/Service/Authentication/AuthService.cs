using AutoMapper;
using FluentValidation;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Claims;
using WEB.DOMAIN.Entity.Authentication;
using WEB.DOMAIN.Entity.Generic;
using WEB.DOMAIN.Interface;
using WEB.SERVICES.DTO.Authentication;
using WEB.SERVICES.DTO.Generic;
using WEB.SERVICES.IService.IAuthentication;
using WEB.SERVICES.IService.IGeneric;
using WEB.SERVICES.Service.Generic;
using WEB.UTILITY.Helper;
using WEB.UTILITY.Logger;
using WEB.UTILITY.Security.ISecurity;

namespace WEB.SERVICES.Service.Authentication
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
                        .Fail(errors.Select(x => x.ErrorMessage).ToList()));
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
                    .Where(a => a.Username == username && a.User.IsActive)
                    .Include(a => a.User)
                        .ThenInclude(x => x.Role)
                    .FirstOrDefaultAsync(ct);

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

        public async Task<Either<ApiResponse<string>, ApiResponse<SessionInfoDto>>> GetSessionInfoAsync(
            ClaimsPrincipal user,
            bool isKeepAlive = false,
            CancellationToken ct = default)
        {
            try
            {
                string msg = ValidateAccessToken(true);
                if (!string.IsNullOrEmpty(msg))
                    return Prelude.Left(ApiResponse<string>.Fail(
                        [msg],
                        HttpStatusCode.NotFound));

                if (isKeepAlive)
                {
                    var jti = _tokenLifecycleService.GetJtiFromToken(_userContextService.AccessToken);
                    if (jti == null)
                        return Prelude.Left(ApiResponse<string>.Fail(["Invalid access token jti"], HttpStatusCode.NotFound));
                    var session = await _tokenLifecycleService.GetActiveSessionAsync(jti, ct);
                    if (session == null || session.IsRevoked || session.RefreshTokenExpiry < DateTime.UtcNow)
                    {
                        return Prelude.Left(ApiResponse<string>.Fail(["No active session"], HttpStatusCode.NotFound));
                    }
                    await _tokenLifecycleService.TouchSessionAsync(session.TokenID, ct);
                }
                return Prelude.Right(ApiResponse<SessionInfoDto>.Ok(null, HttpStatusCode.NoContent));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internal Server Error");
                return Prelude.Left(ApiResponse<string>.Fail(["Internal Server Error"], HttpStatusCode.InternalServerError));
            }
        }

        public async Task<Either<ApiResponse<string>, ApiResponse<SessionInfoDto>>> TryCreateSessionAsync(Guid jti, CancellationToken ct = default)
        {
            try
            {
                var session = await _tokenLifecycleService.GetByTokenIdAsync(jti, ct);
                if (session == null || session.IsRevoked || session.RefreshTokenExpiry < DateTime.UtcNow)
                {
                    return Prelude.Left(ApiResponse<string>.Fail(["Invalid or expired session"], HttpStatusCode.Unauthorized));
                }

                var newToken = _tokenService.GenerateAccessToken(session.User);

                await _tokenLifecycleService.UpdateAccessTokenJtiAsync(session.TokenID, newToken.jti, ct);

                var response = new SessionInfoDto
                {
                    AccessToken = newToken.accessToken,
                    RefreshToken = session.RefreshToken
                };

                return Prelude.Right(ApiResponse<SessionInfoDto>.Ok(response));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating session for access token ID: {jti}");
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

                if (await _tokenLifecycleService.IsActiveLogin(user.UserID, ct))
                {
                    return Prelude.Left(ApiResponse<string>.Fail(["This account is currently logged in. Please log out before attempting to log in again."], HttpStatusCode.Conflict));
                }

                var token = _tokenService.GenerateAccessToken(user);

                var tokenRecord = await _tokenLifecycleService.IssueTokenAsync(user, token.jti, ct);
                if (tokenRecord == null)
                {
                    return Prelude.Left(ApiResponse<string>.Fail(["Not able to issue token"]));
                }
                var response = new SessionInfoDto
                {
                    AccessToken = token.accessToken,
                    RefreshToken = tokenRecord.RefreshToken
                };
                return Prelude.Right(ApiResponse<SessionInfoDto>.Ok(response, HttpStatusCode.OK, "Login Successful"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Login error for user: {authDto.Username}");
                return Prelude.Left(ApiResponse<string>.Fail(["Internal Server Error"], HttpStatusCode.InternalServerError));
            }
        }
        public async Task<Either<ApiResponse<string>, ApiResponse<SessionInfoDto>>> TryRefreshTokenAsync(string? refreshToken, CancellationToken ct = default)
        {
            try
            {
                if (string.IsNullOrEmpty(refreshToken))
                    return Prelude.Left(ApiResponse<string>.Fail(
                        ["Empty refresh token"],
                        HttpStatusCode.NotFound));

                string msg = ValidateAccessToken(false);
                if (!string.IsNullOrEmpty(msg))
                    return Prelude.Left(ApiResponse<string>.Fail(
                        [msg],
                        HttpStatusCode.NotFound));

                var token = await _tokenLifecycleService.GetByRefreshTokenAsync(refreshToken);
                if (token == null || token.RefreshTokenExpiry < DateTime.UtcNow)
                    return Prelude.Left(ApiResponse<string>.Fail(["Invalid or expired refresh token"], HttpStatusCode.NotFound));

                var newToken = _tokenService.GenerateAccessToken(token.User);

                var updatedToken = await _tokenLifecycleService.RotateRefreshTokenAsync(token.TokenID, newToken.jti, ct);
                if (updatedToken == null)
                    return Prelude.Left(ApiResponse<string>.Fail(["Not able to generate new access token"], HttpStatusCode.InternalServerError));

                var response = new SessionInfoDto
                {
                    AccessToken = newToken.accessToken,
                    RefreshToken = token.RefreshToken
                };
                return Prelude.Right(ApiResponse<SessionInfoDto>.Ok(response, HttpStatusCode.OK, "New Access Token has been issued"));
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

        public async Task<Either<ApiResponse<string>, ApiResponse<string>>> TryValidateAccessToken(CancellationToken ct)
        {
            string msg = ValidateAccessToken(true);
            if (!string.IsNullOrEmpty(msg))
                return Prelude.Left(ApiResponse<string>.Fail(
                    [msg],
                    HttpStatusCode.NotFound));
            return Prelude.Right(ApiResponse<string>.Ok("Access token is valid"));
        }

        private string ValidateAccessToken(bool isValidLifetime)
        {
            string errMsg = string.Empty;
            ClaimsPrincipal prin = null;
            var jti = _tokenService.ValidateAccessToken(_userContextService.AccessToken, isValidLifetime);
            if (jti == null)
            {
                errMsg = "Invalid access token";
            }
            try
            {
                var principal = jti(); // Invoke the delegate
                if (principal == null)
                {
                    errMsg = "Access token validation returned null principal";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Access token validation failed.");
                errMsg = "Access token validation failed";
            }
            return errMsg;
        }
    }
}
