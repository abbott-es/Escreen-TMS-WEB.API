using LanguageExt;
using WEB.DOMAIN.Entity;
using WEB.SERVICES.DTO;

namespace WEB.SERVICES.IService
{
    public interface IAuthService
    {
        Task<Either<string, Guid>> CreateUserAsync(UserDto authDTO, CancellationToken ct = default);
        Task<User?> ValidateCredentialsAsync(string username, string encryptedPassword, CancellationToken ct = default);
        Task UpdateLastLoginAsync(string username, CancellationToken ct = default);
    }
}
