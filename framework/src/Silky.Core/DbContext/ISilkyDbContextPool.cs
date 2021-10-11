using System;
using System.Threading;
using System.Threading.Tasks;

namespace Silky.Core.DbContext
{
    public interface ISilkyDbContextPool
    {
        void EnsureDbContextAddToPools();

        int SavePoolNow();

        int SavePoolNow(bool acceptAllChangesOnSuccess);

        Task<int> SavePoolNowAsync(CancellationToken cancellationToken = default);

        Task<int> SavePoolNowAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default);

        void BeginTransaction(bool ensureTransaction = false);

        void CommitTransaction(bool isManualSaveChanges = true, Exception exception = default,
            bool withCloseAll = false);

        void CloseAll();
    }
}