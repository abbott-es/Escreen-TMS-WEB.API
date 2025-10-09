using LanguageExt;
using WEB.DOMAIN.Entity;
using WEB.SERVICES.DTO;

namespace WEB.SERVICES.IService
{
    public interface IUserService : IGenericService<UserDto>
    {
        Task<UserDto?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task<User> GetUserByIdAsync(string userId, CancellationToken ct = default);
    }
}
