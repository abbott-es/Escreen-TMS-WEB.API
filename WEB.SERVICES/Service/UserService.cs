using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using WEB.DOMAIN.Entity;
using WEB.DOMAIN.Interface;
using WEB.SERVICES.DTO;
using WEB.SERVICES.IService;
using WEB.UTILITY.Logger;
using WEB.UTILITY.Security.ISecurity;

namespace WEB.SERVICES.Service
{
    public sealed class UserService : BaseService<UserService>, IUserService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<UserInfo> _userInfoRepository;
        private readonly IRepository<Auth> _authRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<UserDto> _validator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRsaEncryptionService _rsaEncryptionService;

        public UserService(
            IUnitOfWork unitOfWork,
            IRepository<UserInfo> userInfoRepository,
            IMapper mapper,
            IValidator<UserDto> validator,
            IAppLogger<UserService> logger,
            IRepository<Auth> authRepository,
            IRsaEncryptionService rsaEncryptionService,
            IRepository<User> userRepository
        ) : base(logger)
        {
            _userInfoRepository = userInfoRepository;
            _mapper = mapper;
            _validator = validator;
            _unitOfWork = unitOfWork;
            _rsaEncryptionService = rsaEncryptionService;
            _authRepository = authRepository;
            _userRepository = userRepository;
        }

        public async Task<UserDto?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            var user = (await _userInfoRepository.GetAllAsync(ct))
                       .FirstOrDefault(u => u.Email == email);
            return user == null ? null : _mapper.Map<UserDto>(user);
        }

        public async Task<User> GetUserByIdAsync(string userID, CancellationToken ct = default)
        {
            try
            {
                var user = await _userRepository
                        .Query(asNoTracking: true)
                        .Include(x => x.Role)
                        .Include(uf => uf.UserInfo)
                        .Include(uf => uf.Auth)
                        .FirstOrDefaultAsync(a => a.UserID == Guid.Parse(userID) && a.IsActive, ct);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetch user for user ID: {userID}");
                throw;
            }
            
        }

        public async Task<UserDto> GetUserDtoByIdAsync(string userID, CancellationToken ct = default)
        {
            try
            {
                var user = await GetUserByIdAsync(userID, ct);
                return _mapper.Map<UserDto>(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetch user for user ID: {userID}");
                throw;
            }

        }
        //sample dapper use
        //public Task<User?> GetUserByIdAsync(int userId, CancellationToken ct = default)
        //{
        //    return _unitOfWork.ExecuteAsync(async (conn, tran, token) =>
        //    {
        //        var sql = "SELECT * FROM Users WHERE Id = @Id";
        //        return await conn.QueryFirstOrDefaultAsync<User>(sql, new { Id = userId }, tran);
        //    }, ct);
        //}

        //public Task<int> CreateUserAsync(User user, CancellationToken ct = default)
        //{
        //    return _unitOfWork.ExecuteAsync(async (conn, tran, token) =>
        //    {
        //        var parameters = new DynamicParameters();
        //        parameters.Add("@Username", user.Username);
        //        parameters.Add("@Email", user.Email);
        //        parameters.Add("@CreatedAt", DateTime.UtcNow);
        //        parameters.Add("@UserId", dbType: DbType.Int32, direction: ParameterDirection.Output);

        //        await conn.ExecuteAsync("sp_CreateUser", parameters, tran, commandType: CommandType.StoredProcedure);

        //        return parameters.Get<int>("@UserId");
        //    }, ct);
        //}


    }
}
