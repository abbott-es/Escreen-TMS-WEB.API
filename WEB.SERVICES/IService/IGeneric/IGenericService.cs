using LanguageExt;
using WEB.SERVICES.DTO.Generic;
using WEB.UTILITY.Helper;
using WEB.UTILITY.Pagination;

namespace WEB.SERVICES.IService.IGeneric
{
    public interface IGenericService<TDto> where TDto : IBaseDto
    {
        Task<Either<ApiResponse<string>, ApiResponse<PaginatedList<TDto>>>> GetByPaginationAsync(Page page,
            CancellationToken ct = default, params string[] includePaths);
        Task<Either<ApiResponse<string>, ApiResponse<TDto>>> GetByIdAsync(GenericFromQueryDto genericQuery, CancellationToken ct = default);
        Task<Either<ApiResponse<string>, ApiResponse<IEnumerable<TDto>>>> GetAllAsync(CancellationToken ct = default,
            params string[] includePaths);
        Task<Either<ApiResponse<string>, ApiResponse<Guid>>> AddAsync(TDto dto, CancellationToken ct = default);
        Task<Either<ApiResponse<string>, ApiResponse<string>>> UpdateAsync(TDto dto, CancellationToken ct = default, params string[] includePaths);
        Task<Either<ApiResponse<string>, ApiResponse<string>>> DeleteListAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    }
}
