using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEB.DOMAIN.Interface
{
    public interface IDapperUnitOfWork : IAsyncDisposable
    {
        Task ExecuteAsync(Func<DbConnection, DbTransaction, CancellationToken, Task> work, CancellationToken ct = default);
        Task<TResult> ExecuteAsync<TResult>(Func<DbConnection, DbTransaction, CancellationToken, Task<TResult>> work, CancellationToken ct = default);
    }
}
