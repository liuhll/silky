using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;
using Silky.Core;
using Silky.Transaction.Configuration;
using Silky.Transaction.Abstraction;
using Silky.Transaction.Abstraction.Participant;

namespace Silky.Transaction.Repository
{
    public static class TransRepositoryStore
    {
        private static ITransRepository _transRepository;
        private static DistributedTransactionOptions _transactionOptions;

        static TransRepositoryStore()
        {
            _transactionOptions =
                EngineContext.Current.GetOptionsMonitor<DistributedTransactionOptions>((options, s) =>
                    _transactionOptions = options);

            _transRepository = EngineContext.Current.Resolve<ITransRepository>();

            if (_transRepository == null)
            {
                throw new TransactionException(
                    "Failed to obtain the distributed log storage repository, please set the distributed transaction log storage module");
            }
        }

        public static async Task CreateTransaction(ITransaction transaction)
        {
            await _transRepository.CreateTransaction(transaction);
        }

        public static async Task<ITransaction> LoadTransaction(string tranId)
        {
            var transaction = await _transRepository.FindByTransId(tranId);
            return transaction is { Status: ActionStage.Delete } ? null : transaction;
        }

        public static async Task UpdateTransactionStatus(ITransaction transaction)
        {
            if (transaction != null)
            {
                await _transRepository.UpdateTransactionStatus(transaction.TransId, transaction.Status);
            }
        }

        public static async Task CreateParticipant(IParticipant participant)
        {
            if (participant != null)
            {
                await _transRepository.CreateParticipant(participant);
            }
        }

        public static async Task<IReadOnlyCollection<IParticipant>> LoadParticipant(string transId,
            string participantId)
        {
            var participant = await _transRepository.FindParticipant(transId, participantId);
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
                if (_transactionOptions.PhyDeleted)
                {
                    await _transRepository.RemoveParticipant(participant.TransId, participant.ParticipantId);
                }
                else
                {
                    await _transRepository.UpdateParticipantStatus(participant.TransId, participant.ParticipantId,
                        ActionStage.Delete);
                }
            }
        }

        public static Task<IReadOnlyCollection<IParticipant>> ListParticipant(DateTime dateTime,
            TransactionType transactionType, int limit)
        {
            return _transRepository.ListParticipant(dateTime, transactionType, limit);
        }

        public static async Task RemoveTransaction(ITransaction transaction)
        {
            if (transaction != null)
            {
                if (_transactionOptions.PhyDeleted)
                {
                    await _transRepository.RemoveTransaction(transaction.TransId);
                }
                else
                {
                    await _transRepository.UpdateTransactionStatus(transaction.TransId, ActionStage.Delete);
                }
            }
        }

        public static async Task<bool> LockParticipant(IParticipant participant)
        {
            if (participant != null)
            {
                return await _transRepository.LockParticipant(participant);
            }

            return false;
        }

        public static async Task<IReadOnlyCollection<ITransaction>> ListLimitByDelay(DateTime dateTime, int limit)
        {
            return await _transRepository.ListLimitByDelay(dateTime, limit);
        }

        public static async Task<bool> ExistParticipantByTransId(string transId)
        {
            return await _transRepository.ExistParticipantByTransId(transId);
        }

        public static async Task<int> RemoveTransactionByDate(DateTime dateTime)
        {
            return await _transRepository.RemoveTransactionByDate(dateTime);
        }

        public static async Task<int> RemoveParticipantByDate(DateTime dateTime)
        {
            return await _transRepository.RemoveParticipantByDate(dateTime);
        }

        public static async Task<IReadOnlyCollection<IParticipant>> ListParticipantByTransId(string transId)
        {
            return await _transRepository.ListParticipantByTransId(transId);
        }
    }
}