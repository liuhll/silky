using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Silky.Transaction.Cache;
using Silky.Transaction.Repository.Spi;
using Silky.Transaction.Repository.Spi.Participant;

namespace Silky.Transaction
{
    public class SilkyTransactionHolder
    {
        private static readonly SilkyTransactionHolder instance = new();

        private static readonly AsyncLocal<ITransaction> CURRENT = new();

        private SilkyTransactionHolder()
        {
        }

        public static SilkyTransactionHolder Instance => instance;

        public void Set(ITransaction transaction)
        {
            CURRENT.Value = transaction;
        }

        public ITransaction CurrentTransaction => CURRENT.Value;


        public void Remove()
        {
            CURRENT.Value = null;
        }

        public async Task CacheParticipant(IParticipant participant)
        {
            if (participant == null) return;
            await ParticipantCacheManager.Instance.CacheParticipant(participant);
        }

        public void RegisterParticipantByNested(string participantId, IParticipant participant)
        {
            if (participant == null) return;
            ParticipantCacheManager.Instance.CacheParticipant(participantId, participant).GetAwaiter().GetResult();
        }

        public void RegisterStarterParticipant(IParticipant participant)
        {
            if (participant == null) return;
            if (CurrentTransaction != null)
            {
                CurrentTransaction.RegisterParticipant(participant);
            }
        }
    }
}