using LanguageExt;
using WEB.SERVICES.DTO.Generic;
using WEB.UTILITY.Helper;

namespace WEB.SERVICES.IService.IGeneric
{
    public interface IUserService
    {
        Task<Either<ApiResponse<string>, ApiResponse<UserDto>>> GetUserDtoByIdAsync(Guid userID, CancellationToken ct = default);
        Task<Either<ApiResponse<string>, ApiResponse<string>>> GetActiveUserRoleAsync(CancellationToken ct = default);
        Task<Either<ApiResponse<string>, ApiResponse<IEnumerable<UserDto>>>> GetAllUserByRoleAsync(GenericFromQueryDto userRoleDto, CancellationToken ct = default);
        Task<Either<ApiResponse<string>, ApiResponse<IEnumerable<DriverHelperDto>>>> GetAllDriver(
            CancellationToken ct = default);
        Task<Either<ApiResponse<string>, ApiResponse<IEnumerable<DriverHelperDto>>>> GetAllHelper(
            CancellationToken ct = default);
    }
}
