using AutoMapper;
using FluentValidation;
using LanguageExt;
using LanguageExt.Pipes;
using Microsoft.EntityFrameworkCore;
using WEB.DOMAIN.Entity;
using WEB.DOMAIN.Interface;
using WEB.SERVICES.DTO;
using WEB.SERVICES.IService;
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
        private readonly IAppLogger<AuthService> _appLogger;
        private readonly IRsaEncryptionService _rsaEncryptionService;

        public AuthService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<UserDto> validator,
            IAppLogger<AuthService> appLogger,
            IRepository<Auth> authRepository,
            IRsaEncryptionService rsaEncryptionService,
            IRepository<User> userRepository,
            IUserContextService userContextService
            ) : base(appLogger)
        {
            _mapper = mapper;
            _validator = validator;
            _unitOfWork = unitOfWork;
            _rsaEncryptionService = rsaEncryptionService;
            _appLogger = appLogger;
            _authRepository = authRepository;
            _userRepository = userRepository;
        }

        public async Task<Either<string, Guid>> CreateUserAsync(UserDto authDTO, CancellationToken ct = default)
        {
            return await ExecuteWithLoggingAsync(async ct =>
            {
                var validate = await _validator.ValidateAsync(authDTO, ct);
                if (!validate.IsValid)
                {
                    var errors = validate.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                    var errorMessage = string.Join("; ", errors.Select(e => $"{e.ErrorMessage}"));
                    _logger.LogWarning($"User creation failed validation: {errorMessage}");
                    return Prelude.Left<string, Guid>(errorMessage);
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
                    throw;
                }

                _logger.LogInformation($"User created successfully: {user.UserID}");
                return Prelude.Right<string, Guid>(user.UserID);
            }, nameof(CreateUserAsync), ct);
        }

        public async Task<User?> ValidateCredentialsAsync(string username, string encryptedPassword, CancellationToken ct = default)
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

        public async Task UpdateLastLoginAsync(string username, CancellationToken ct = default)
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
    }
}
