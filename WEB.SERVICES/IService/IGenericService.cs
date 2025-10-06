using LanguageExt;

namespace WEB.SERVICES.IService
{
    public interface IGenericService<TDto> where TDto : class
    {
        Task<Either<string, TDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<Either<string, IEnumerable<TDto>>> GetAllAsync(CancellationToken ct = default);
        Task<Either<string, Guid>> AddAsync(TDto dto, CancellationToken ct = default);
        Task<Either<string, bool>> UpdateAsync(TDto dto, CancellationToken ct = default);
        Task<Either<string, bool>> DeleteListAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    }
}
