using LanguageExt;
using WEB.SERVICES.DTO;

namespace WEB.SERVICES.IService
{
    public interface IUserService : IGenericService<UserDto>
    {
        Task<UserDto?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task<Either<string, Guid>> CreateUserAsync(UserDto userDto, CancellationToken ct = default);
    }
}
