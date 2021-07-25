using System.Threading.Tasks;
using System.Transactions;
using Silky.Core;
using Silky.Core.DynamicProxy;
using Silky.Transaction.Configuration;
using Silky.Transaction.Repository.Spi;
using Silky.Transaction.Repository.Spi.Participant;

namespace Silky.Transaction.Repository
{
    public static class TransRepositoryStore
    {
        private static ITransRepository _transRepository;

        static TransRepositoryStore()
        {
            var transactionOptions = EngineContext.Current.GetOptions<DistributedTransactionOptions>();
            _transRepository =
                EngineContext.Current.ResolveNamed<ITransRepository>(transactionOptions.UndoLogRepositorySupport.ToString());

            if (_transRepository == null)
            {
                throw new TransactionException(
                    "Failed to obtain the distributed log storage repository, please set the distributed transaction log storage module");
            }
        }

        public static async Task SaveTransaction(ITransaction transaction)
        {
            await _transRepository.SaveTransaction(transaction);
        }

        public static async Task<ITransaction> LoadTransaction(string tranId)
        {
            return await _transRepository.FindByTransId(tranId);
        }

        public static async Task UpdateTransactionStatus(ITransaction transaction)
        {
            if (transaction != null)
            {
                await _transRepository.UpdateTransactionStatus(transaction.TransId, transaction.Status);
            }
        }

        public static async Task SaveParticipant(IParticipant participant)
        {
            if (participant != null)
            {
                await _transRepository.SaveParticipant(participant);
            }
        }

        public static async Task<IParticipant> LoadParticipant(string transId, string participantId,
            ISilkyMethodInvocation invocation)
        {
            var participant = await _transRepository.FindParticipant(transId, participantId);
            participant.Invocation = invocation;
            return participant;
        }


        public static async Task UpdateParticipantStatus(IParticipant participant)
        {
            if (participant != null)
            {
                await _transRepository.UpdateParticipantStatus(participant.TransId, participant.ParticipantId,
                    participant.Status);
            }
        }

        public static async Task RemoveParticipant(IParticipant participant)
        {
            if (participant != null)
            {
                await _transRepository.RemoveParticipant(participant.TransId, participant.ParticipantId);
            }
        }
    }
}