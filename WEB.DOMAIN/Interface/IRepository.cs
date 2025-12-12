using System.Linq.Expressions;
using WEB.UTILITY.Pagination;

namespace WEB.DOMAIN.Interface
{
    public interface IRepository<T> where T : class
    {
        Task<PaginatedList<T>> GetByPaginationAsync(
            Page page,
            Expression<Func<T, bool>>? predicate = null,
            CancellationToken ct = default,
            bool asNoTracking = true,
            params string[] includePaths);
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default, bool asNoTracking = true, params string[] includePaths);

        // If your entities always use Guid Id:
        Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default, params string[] includePaths);
        Task<T?> GetByKeysAsync(CancellationToken ct = default, params object?[] keyValues);
        Task AddAsync(T entity, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
        void Update(T entity, params Expression<Func<T, object>>[] modifiedNavigations);
        void Delete(T entity);
        Task DeleteRangeAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
        IQueryable<T> Query(bool asNoTracking = true);
    }
}
