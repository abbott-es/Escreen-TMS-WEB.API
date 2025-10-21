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
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<UserInfo> _userInfoRepository;
        private readonly IMapper _mapper;
        private readonly IUserContextService _userContextService;

        public UserService(
            IRepository<UserInfo> userInfoRepository,
            IMapper mapper,
            IAppLogger<UserService> logger,
            IRepository<User> userRepository,
            IUserContextService userContextService,
            IUnitOfWork unitOfWork
        ) : base(logger)
        {
            _userInfoRepository = userInfoRepository;
            _mapper = mapper;
            _userRepository = userRepository;
            _userContextService = userContextService;
            _unitOfWork = unitOfWork;
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

        public async Task<Either<ApiResponse<string>, ApiResponse<IEnumerable<UserDto>>>> GetAllUserByRoleAsync(GenericFromQueryDto userRoleDto, CancellationToken ct = default)
        {
            try
            {
                var entities = await _userRepository.GetAllAsync(d => d.RoleID == userRoleDto.ID, ct, true, userRoleDto.Includes);
                return Prelude.Right(ApiResponse<IEnumerable<UserDto>>.Ok(_mapper.Map<IEnumerable<UserDto>>(entities).ToList()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all driver");
                return Prelude.Left(ApiResponse<string>.Fail(["Internal Server Error"], HttpStatusCode.InternalServerError));
            }
        }

        public async Task<Either<ApiResponse<string>, ApiResponse<IEnumerable<UserInfoDto>>>> GetAllDriver(CancellationToken ct = default)
        {
            try
            {
                var entities = await _userRepository.GetAllAsync(d => d.RoleID == Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"), ct, true, ["UserInfo"]);
                return Prelude.Right(ApiResponse<IEnumerable<UserInfoDto>>.Ok(_mapper.Map<IEnumerable<UserInfoDto>>(entities.Select(x => x.UserInfo)).ToList()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all driver");
                return Prelude.Left(ApiResponse<string>.Fail(["Internal Server Error"], HttpStatusCode.InternalServerError));
            }
        }

        public async Task<Either<ApiResponse<string>, ApiResponse<IEnumerable<UserInfoDto>>>> GetAllHelper(CancellationToken ct = default)
        {
            try
            {
                var entities = await _userRepository.GetAllAsync(d => d.RoleID == Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd"), ct, true, ["UserInfo"]);
                return Prelude.Right(ApiResponse<IEnumerable<UserInfoDto>>.Ok(_mapper.Map<IEnumerable<UserInfoDto>>(entities.Select(x => x.UserInfo)).ToList()));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching all helper");
                return Prelude.Left(ApiResponse<string>.Fail(["Internal Server Error"], HttpStatusCode.InternalServerError));
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
