
using LanguageExt;
using WEB.UTILITY.Helper;

namespace WEB.SERVICES.IService.IGeneric
{
    public interface IGatewayService
    {
        Task<Either<ApiResponse<string>, ApiResponse<string>>> GetGatewayUrlByKeyAsync(string keyName,
            CancellationToken ct);
    }
}
