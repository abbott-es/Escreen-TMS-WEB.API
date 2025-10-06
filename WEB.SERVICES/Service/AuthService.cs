using AutoMapper;
using FluentValidation;
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
        private readonly IRepository<Auth> _authRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<UserDto> _validator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAppLogger<UserInfo> _logger;
        private readonly IRsaEncryptionService _rsaEncryptionService;

        public AuthService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IValidator<UserDto> validator,
            IAppLogger<UserInfo> logger,
            IRepository<Auth> authRepository,
            IRsaEncryptionService rsaEncryptionService)
        {
            _mapper = mapper;
            _validator = validator;
            _unitOfWork = unitOfWork;
            _rsaEncryptionService = rsaEncryptionService;
            _logger = logger;
            _authRepository = authRepository;
        }
        public async Task<UserInfo?> ValidateCredentialsAsync(string username, string encryptedPassword)
        {
            return await _unitOfWork.ExecuteReadOnlyAsync(async ct =>
            {
                var auth = await _authRepository
                    .Query(asNoTracking: true)
                    .Include(a => a.User)
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
