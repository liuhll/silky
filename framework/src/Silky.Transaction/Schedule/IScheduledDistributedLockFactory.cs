using System.Threading.Tasks;
using Medallion.Threading;
using Silky.Transaction.Abstraction;

namespace Silky.Transaction.Schedule
{
    public interface IScheduledDistributedLockFactory
    {
        Task<IDistributedLockProvider> CreateDistributedLockProvider(TransRepositorySupport transRepositorySupport);
    }
}