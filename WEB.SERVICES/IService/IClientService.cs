
using LanguageExt;
using WEB.UTILITY.Helper;

namespace WEB.SERVICES.IService
{
    public interface IClientService
    {
        Task<Either<ApiResponse<string>, ApiResponse<string>>> DeleteListAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    }
}
