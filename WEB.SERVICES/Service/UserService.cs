using AutoMapper;
using LanguageExt;
using Microsoft.EntityFrameworkCore;
using System.Net;
using WEB.DOMAIN.Entity;
using WEB.DOMAIN.Interface;
using WEB.SERVICES.DTO;
using WEB.SERVICES.IService;
using WEB.UTILITY.Helper;
using WEB.UTILITY.Logger;

namespace WEB.SERVICES.Service
{
    public sealed class UserService : BaseService<UserService>, IUserService
    {
        private readonly IRepository<User> _userRepository;
        private readonly IRepository<UserInfo> _userInfoRepository;
        private readonly IMapper _mapper;
        private readonly IUserContextService _userContextService;

        public UserService(
            IRepository<UserInfo> userInfoRepository,
            IMapper mapper,
            IAppLogger<UserService> logger,
            IRepository<User> userRepository,
            IUserContextService userContextService
        ) : base(logger)
        {
            _userInfoRepository = userInfoRepository;
            _mapper = mapper;
            _userRepository = userRepository;
            _userContextService = userContextService;
        }

        private async Task<User> GetUserByIdAsync(Guid userID, CancellationToken ct = default)
        {
            try
            {
                var user = await _userRepository
                        .Query(asNoTracking: true)
                        .Include(x => x.Role)
                        .Include(uf => uf.UserInfo)
                        .Include(uf => uf.Auth)
                        .FirstOrDefaultAsync(a => a.UserID == userID && a.IsActive, ct);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetch user for user ID: {userID}");
                return null;
            }
        }

        public async Task<Either<ApiResponse<string>, ApiResponse<UserDto>>> GetUserDtoByIdAsync(Guid userID, CancellationToken ct = default)
        {
            var user = await GetUserByIdAsync(userID, ct);
            if (user == null)
                return Prelude.Left(ApiResponse<string>.Fail(["Invalid User ID"], HttpStatusCode.NotFound));

            return Prelude.Right(ApiResponse<UserDto>.Ok(_mapper.Map<UserDto>(user), HttpStatusCode.OK, "User successfully retrieved"));
        }

        public async Task<Either<ApiResponse<string>, ApiResponse<string>>> GetActiveUserRoleAsync(CancellationToken ct = default)
        {
            var user = await GetUserByIdAsync(Guid.Parse(_userContextService.UserId), ct);
            if (user == null)
                return Prelude.Left(ApiResponse<string>.Fail(["Invalid Authenticated User"], HttpStatusCode.NotFound));

            return Prelude.Right(ApiResponse<string>.Ok(user.Role.RoleName, HttpStatusCode.OK, "User role successfully retrieved"));
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
