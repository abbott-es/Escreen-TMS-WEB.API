using WEB.DOMAIN.Entity;

namespace WEB.DOMAIN.Interface
{
    public interface IBaseEntityRepository<T> where T : BaseEntity
    {
        void SoftDelete(T entity);
        Task SoftDeleteListAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    }
}
