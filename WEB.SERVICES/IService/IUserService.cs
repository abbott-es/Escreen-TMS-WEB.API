using LanguageExt;
using WEB.SERVICES.DTO;
using WEB.UTILITY.Helper;

namespace WEB.SERVICES.IService
{
    public interface IUserService
    {
        Task<Either<ApiResponse<string>, ApiResponse<UserDto>>> GetUserDtoByIdAsync(string userID, CancellationToken ct = default);
        Task<Either<ApiResponse<string>, ApiResponse<string>>> GetActiveUserRoleAsync(CancellationToken ct = default);
    }
}
