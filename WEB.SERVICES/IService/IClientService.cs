
namespace WEB.SERVICES.IService
{
    public interface IClientService
    {
        Task<bool> DeleteListAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    }
}
