using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using WEB.DOMAIN.Interface;
using WEB.DOMAIN.Resolver;

namespace WEB.DAL.Repository
{
    public class Repository<T> : IRepository<T> where T : class, IEntity
    {
        protected readonly AppDbContext.WebApiDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(AppDbContext.WebApiDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default, bool asNoTracking = true, params string[] includePaths)
        {
            IQueryable<T> query = _dbSet;

            if (asNoTracking)
                query = query.AsNoTracking();

            if (predicate != null)
            {
                query = query.Where(predicate);
            }
            if (includePaths != null)
            {
                foreach (var include in includePaths)
                    query = query.Include(include);
            }

            return await query.ToListAsync(ct);
        }


        public async Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default, params string[] includePaths)
        {
            IQueryable<T> query = _dbSet.AsNoTracking();
            if (includePaths?.Length > 0)
            {
                foreach (var include in includePaths)
                {
                    query = query.Include(include);
                }
            }
            var keyName = EntityKeyResolver.GetMappedKeyPropertyName<T>();
            return await query.Where(e => EF.Property<Guid>(e, keyName) != Guid.Empty && EF.Property<Guid>(e, keyName) == id).FirstOrDefaultAsync(ct);
        }

        public async Task<T?> GetByKeysAsync(CancellationToken ct = default, params object?[] keyValues)
            => await _dbSet.FindAsync(keyValues, ct);

        public Task AddAsync(T entity, CancellationToken ct = default)
            => _dbSet.AddAsync(entity, ct).AsTask();

        public Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default)
            => _dbSet.AddRangeAsync(entities, ct);

        public void Update(T entity, params Expression<Func<T, object>>[] modifiedNavigations)//which include to modify
        {
            _dbSet.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;

            foreach (var nav in modifiedNavigations)
            {
                var memberExpression = nav.Body as MemberExpression ?? (nav.Body is UnaryExpression unary ? unary.Operand as MemberExpression : null);

                if (memberExpression == null) continue;

                var propertyInfo = memberExpression.Member as PropertyInfo;
                if (propertyInfo == null) continue;

                var value = propertyInfo.GetValue(entity);

                if (value != null)
                {
                    var propertyType = propertyInfo.PropertyType;

                    if (typeof(IEnumerable).IsAssignableFrom(propertyType) && propertyType != typeof(string))
                    {
                        // It's a collection navigation
                        var collection = value as IEnumerable;
                        if (collection != null)
                        {
                            foreach (var item in collection)
                            {
                                var itemType = item.GetType();
                                var idProperty = itemType.GetProperty("Id"); // Assumes the ID is named "Id"

                                if (idProperty != null)
                                {
                                    var idValue = idProperty.GetValue(item);

                                    if (idValue == null || (idValue is Guid guid && guid == Guid.Empty))
                                    {
                                        _context.Entry(item).State = EntityState.Added;
                                    }
                                    else
                                    {
                                        _context.Entry(item).State = EntityState.Modified;
                                    }
                                }
                                else
                                {
                                    _context.Entry(item).State = EntityState.Modified;
                                }
                            }
                        }
                    }
                    else
                    {
                        // It's a reference navigation
                        _context.Entry(entity).Reference(nav).TargetEntry.State = EntityState.Modified;
                    }
                }
            }
        }

        public void Delete(T entity) => _dbSet.Remove(entity);

        public async Task DeleteRangeAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
        {
            if (ids == null || !ids.Any())
                return;

            // Get matching entities
            var keyName = EntityKeyResolver.GetMappedKeyPropertyName<T>();
            var entities = await _dbSet
                .Where(e => EF.Property<Guid>(e, keyName) != Guid.Empty && ids.Contains(EF.Property<Guid>(e, keyName)))
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