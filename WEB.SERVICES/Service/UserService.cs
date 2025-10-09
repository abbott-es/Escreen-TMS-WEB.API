using AutoMapper;
using Dapper;
using FluentValidation;
using LanguageExt;
using LanguageExt.Pipes;
using System.ComponentModel.DataAnnotations;
using System.Data;
using Microsoft.EntityFrameworkCore;
using WEB.DAL;
using WEB.DAL.Repository;
using WEB.DOMAIN.Entity;
using WEB.DOMAIN.Interface;
using WEB.SERVICES.DTO;
using WEB.SERVICES.IService;
using WEB.UTILITY.Logger;
using WEB.UTILITY.Security.ISecurity;
using static Dapper.SqlMapper;

namespace WEB.SERVICES.Service
{
    public sealed class UserService : GenericService<UserInfo, UserDto>, IUserService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<UserInfo> _userInfoRepository;
        private readonly IRepository<Auth> _authRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<UserDto> _validator;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAppLogger<UserInfo> _logger;
        private readonly IRsaEncryptionService _rsaEncryptionService;

        public UserService(
            IUnitOfWork unitOfWork,
            IRepository<UserInfo> userInfoRepository,
            IMapper mapper,
            IValidator<UserDto> validator,
            IAppLogger<UserInfo> logger,
            IRepository<Auth> authRepository,
            IRsaEncryptionService rsaEncryptionService,
            IRepository<User> userRepository
        ) : base(unitOfWork, userInfoRepository, mapper, validator, logger)
        {
            _userInfoRepository = userInfoRepository;
            _mapper = mapper;
            _validator = validator;
            _unitOfWork = unitOfWork;
            _rsaEncryptionService = rsaEncryptionService;
            _logger = logger;
            _authRepository = authRepository;
            _userRepository = userRepository;
        }

        public async Task<UserDto?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            var user = (await _userInfoRepository.GetAllAsync(ct))
                       .FirstOrDefault(u => u.Email == email);
            return user == null ? null : _mapper.Map<UserDto>(user);
        }
        public async Task<User> GetUserByIdAsync(string userId, CancellationToken ct = default)
        {
            return await _unitOfWork.ExecuteReadOnlyAsync(async ct =>
            {
                var auth = await _authRepository
                    .Query(asNoTracking: true)
                    .Include(a => a.User)
                    .ThenInclude(x => x.Role)
                    .FirstOrDefaultAsync(a => a.UserID == Guid.Parse(userId) && a.User.IsActive, ct);
                return auth.User;
            });
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
