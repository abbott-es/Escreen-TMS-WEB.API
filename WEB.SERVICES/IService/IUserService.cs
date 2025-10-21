using LanguageExt;
using WEB.SERVICES.DTO;
using WEB.UTILITY.Helper;

namespace WEB.SERVICES.IService
{
    public interface IUserService
    {
        Task<Either<ApiResponse<string>, ApiResponse<UserDto>>> GetUserDtoByIdAsync(Guid userID, CancellationToken ct = default);
        Task<Either<ApiResponse<string>, ApiResponse<string>>> GetActiveUserRoleAsync(CancellationToken ct = default);
        Task<Either<ApiResponse<string>, ApiResponse<IEnumerable<UserDto>>>> GetAllUserByRoleAsync(GenericFromQueryDto userRoleDto, CancellationToken ct = default);
    }
}
