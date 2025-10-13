using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;
using WEB.DOMAIN.Interface;

namespace WEB.DAL.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly AppDbContext.WebApiDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(AppDbContext.WebApiDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default, bool asNoTracking = true, params string[] includePaths)
        {
            IQueryable<T> query = _dbSet;

            if (asNoTracking)
                query = query.AsNoTracking();

            foreach (var include in includePaths)
                query = query.Include(include);

            return await query.ToListAsync(ct);
        }

        public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            // If your key is always Guid Id, this is fine. Otherwise, prefer the FindAsync(params object?[])
            return await _dbSet.FindAsync(new object?[] { id }, ct);
        }

        public async Task<T?> GetByKeysAsync(CancellationToken ct = default, params object?[] keyValues)
            => await _dbSet.FindAsync(keyValues, ct);

        public Task AddAsync(T entity, CancellationToken ct = default)
            => _dbSet.AddAsync(entity, ct).AsTask();

        public Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
            => _dbSet.AddRangeAsync(entities, ct);

        public void Update(T entity) => _dbSet.Update(entity);

        public void Delete(T entity) => _dbSet.Remove(entity);

        public async Task DeleteRangeAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
        {
            if (ids == null || !ids.Any())
                return;

            // Get matching entities
            var entities = await _dbSet
                .Where(e => EF.Property<Guid>(e, "Id") != Guid.Empty && ids.Contains(EF.Property<Guid>(e, "Id")))
                .ToListAsync(ct);

            if (entities.Count == 0)
                return;

            _dbSet.RemoveRange(entities);
        }
        public IQueryable<T> Query(bool asNoTracking = true)
        {
            return asNoTracking ? _dbSet.AsNoTracking() : _dbSet;
        }
    }
}