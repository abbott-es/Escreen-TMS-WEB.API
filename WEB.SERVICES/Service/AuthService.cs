using AutoMapper;
using FluentValidation;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using WEB.DAL.Repository;
using WEB.DOMAIN.Entity;
using WEB.DOMAIN.Interface;
using WEB.SERVICES.DTO;
using WEB.SERVICES.IService;
using WEB.UTILITY.Logger;
using WEB.UTILITY.Security.ISecurity;

namespace WEB.SERVICES.Service
{
    public class AuthService : IAuthService
    {
        private readonly IRepository<User> _userRepository;
        //private readonly IRepository<UserInfo> _userInfoRepository;
        private readonly IRepository<Auth> _authRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<UserDto> _validator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAppLogger<Auth> _logger;
        private readonly IRsaEncryptionService _rsaEncryptionService;

        public AuthService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<UserDto> validator,
            IAppLogger<Auth> logger,
            IRepository<Auth> authRepository,
            IRsaEncryptionService rsaEncryptionService,
            IRepository<User> userRepository,
            IUserContextService userContextService
            //IRepository<UserInfo> userInfoRepository
            )
        {
            _mapper = mapper;
            _validator = validator;
            _unitOfWork = unitOfWork;
            _rsaEncryptionService = rsaEncryptionService;
            _logger = logger;
            _authRepository = authRepository;
            _userRepository = userRepository;
            //_userInfoRepository = userInfoRepository;
        }

        public async Task<Either<string, Guid>> CreateUserAsync(UserDto authDTO, CancellationToken ct = default)
        {
            var validate = await _validator.ValidateAsync(authDTO, ct);
            if (!validate.IsValid)
            {
                var errors = validate.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                return string.Join("; ", errors.Select(e => $"{e.ErrorMessage}"));
            }
            var user = _mapper.Map<User>(authDTO);
            await _unitOfWork.ExecuteAsync(async c =>
            {
                await _userRepository.AddAsync(user, c);
            }, ct);
            return user.UserID;
        }
        public async Task<User?> ValidateCredentialsAsync(string username, string encryptedPassword, CancellationToken ct = default)
        {
            return await _unitOfWork.ExecuteReadOnlyAsync(async ct =>
            {
                var auth = await _authRepository
                    .Query(asNoTracking: true)
                    .Include(a => a.User)
                    .ThenInclude(x => x.Role)
                    .FirstOrDefaultAsync(a => a.Username == username && a.User.IsActive, ct);

                if (auth == null)
                    return null;

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
                return isValid ? auth.User : null;
            });
        }
    }
}
