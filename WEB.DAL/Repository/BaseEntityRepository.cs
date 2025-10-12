using Microsoft.EntityFrameworkCore;
using WEB.DOMAIN.Entity;
using WEB.DOMAIN.Interface;

namespace WEB.DAL.Repository
{
    public class BaseEntityRepository<T> : IBaseEntityRepository<T> where T : BaseEntity
    {
        protected readonly AppDbContext.WebApiDbContext _context;
        protected readonly DbSet<T> _dbSet;
        public BaseEntityRepository(AppDbContext.WebApiDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
        }

        public void SoftDelete(T entity)
        {
            entity.IsActive = false;
            entity.DeletedAt = DateTime.UtcNow;
            _dbSet.Update(entity);
        }

        public async Task SoftDeleteListAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
        {
            if (ids == null || !ids.Any())
                return;

            var entities = await _dbSet
                .Where(e => ids.Contains(EF.Property<Guid>(e, "ID")))
                .ToListAsync(ct);

            if (!entities.Any())
                return;

            foreach (var entity in entities)
            {
                entity.IsActive = false;
                entity.DeletedAt = DateTime.UtcNow;
                _context.Entry(entity).State = EntityState.Modified;
            }
        }
    }
}
