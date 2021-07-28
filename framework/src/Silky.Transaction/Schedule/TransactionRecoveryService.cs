using System.Threading.Tasks;
using Silky.Core.DependencyInjection;
using Silky.Transaction.Abstraction.Participant;

namespace Silky.Transaction.Schedule
{
    public class TransactionRecoveryService : ITransientDependency
    {
        public async Task Cancel(IParticipant participant)
        {
            throw new System.NotImplementedException();
        }

        public async Task Confirm(IParticipant participant)
        {
            throw new System.NotImplementedException();
        }
    }
}