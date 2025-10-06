using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEB.DOMAIN.Interface
{
    public interface IEFUnitOfWork : IAsyncDisposable
    {
        Task ExecuteAsync(Func<CancellationToken, Task> work, CancellationToken ct = default);
        Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> work, CancellationToken ct = default);

        Task ExecuteReadOnlyAsync(Func<CancellationToken, Task> work, CancellationToken ct = default);
        Task<TResult> ExecuteReadOnlyAsync<TResult>(Func<CancellationToken, Task<TResult>> work, CancellationToken ct = default);
    }
}
