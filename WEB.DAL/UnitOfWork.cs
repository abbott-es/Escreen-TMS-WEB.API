using System.Data;
using System.Data.Common;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using WEB.DOMAIN.Interface;

namespace WEB.DAL
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext.WebApiDbContext _ctx;
        private static readonly AsyncLocal<int> AmbientDepth = new();

        public UnitOfWork(AppDbContext.WebApiDbContext ctx) => _ctx = ctx;

        public Task ExecuteReadOnlyAsync(Func<CancellationToken, Task> work, CancellationToken ct = default) => work(ct);
        public Task<TResult> ExecuteReadOnlyAsync<TResult>(Func<CancellationToken, Task<TResult>> work, CancellationToken ct = default) => work(ct);

        public Task ExecuteAsync(Func<CancellationToken, Task> work, CancellationToken ct = default)
            => ExecuteCoreAsync<object?>(async (_, __, c) => { await work(c); return null; }, ct);

        public Task<TResult> ExecuteAsync<TResult>(Func<CancellationToken, Task<TResult>> work, CancellationToken ct = default)
            => ExecuteCoreAsync(async (_, __, c) => await work(c), ct);

        public Task ExecuteAsync(Func<DbConnection, DbTransaction, CancellationToken, Task> work, CancellationToken ct = default)
            => ExecuteCoreAsync<object?>(async (conn, tx, c) => { await work(conn, tx, c); return null; }, ct);

        public Task<TResult> ExecuteAsync<TResult>(Func<DbConnection, DbTransaction, CancellationToken, Task<TResult>> work, CancellationToken ct = default)
            => ExecuteCoreAsync(work, ct);

        private async Task<TResult> ExecuteCoreAsync<TResult>(
            Func<DbConnection, DbTransaction, CancellationToken, Task<TResult>> work,
            CancellationToken ct)
        {
            // Optional: guard against non-relational provider (e.g., InMemory)
            if (!_ctx.Database.IsRelational())
                throw new InvalidOperationException("This UnitOfWork requires a relational EF Core provider.");

            var strategy = _ctx.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                var isOuter = AmbientDepth.Value == 0;
                AmbientDepth.Value++;

                try
                {
                    if (isOuter)
                    {
                        await using var tx = await _ctx.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);

                        var conn = _ctx.Database.GetDbConnection();
                        var dbtx = tx.GetDbTransaction();               // <-- use tx here (never null)

                        var result = await work(conn, dbtx, ct);

                        await _ctx.SaveChangesAsync(ct);
                        await tx.CommitAsync(ct);
                        return result;
                    }
                    else
                    {
                        // Reuse ambient transaction opened by outer scope
                        var conn = _ctx.Database.GetDbConnection();

                        // At this point, CurrentTransaction MUST be non-null because the outer started it.
                        var current = _ctx.Database.CurrentTransaction
                                      ?? throw new InvalidOperationException("No ambient EF transaction found in nested scope.");
                        var dbtx = current.GetDbTransaction();

                        return await work(conn, dbtx, ct);
                    }
                }
                catch
                {
                    if (_ctx.Database.CurrentTransaction is not null)
                        await _ctx.Database.RollbackTransactionAsync(ct);
                    throw;
                }
                finally
                {
                    AmbientDepth.Value--;
                }
            });
        }

        public ValueTask DisposeAsync() => _ctx.DisposeAsync();
    }
}
