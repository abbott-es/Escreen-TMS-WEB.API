using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace WEB.DOMAIN.Interface
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default, bool asNoTracking = true,
            params string[] includePaths);

        // If your entities always use Guid Id:
        Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);

        Task<T?> GetByKeysAsync(CancellationToken ct = default, params object?[] keyValues);

        Task AddAsync(T entity, CancellationToken ct = default);
        Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);

        void Update(T entity);
        void Delete(T entity);
        Task DeleteRangeAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
        IQueryable<T> Query(bool asNoTracking = true);
    }
}
