using System.Data.Common;

namespace WEB.DOMAIN.Interface
{
    public interface IUnitOfWork : IEFUnitOfWork, IDapperUnitOfWork
    {
        // Optionally add shared methods or markers here
    }
}
