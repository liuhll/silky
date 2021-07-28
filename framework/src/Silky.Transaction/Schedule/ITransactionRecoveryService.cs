using System.Threading.Tasks;
using Silky.Core.DependencyInjection;
using Silky.Transaction.Abstraction.Participant;

namespace Silky.Transaction.Schedule
{
    public interface ITransactionRecoveryService : ITransientDependency
    {
        Task<bool> Cancel(IParticipant participant);

        Task<bool> Confirm(IParticipant participant);
    }
}