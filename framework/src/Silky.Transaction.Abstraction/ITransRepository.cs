using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Silky.Transaction.Abstraction.Participant;

namespace Silky.Transaction.Abstraction
{
    public interface ITransRepository
    {
        Task CreateTransaction([NotNull] ITransaction transaction);

        Task<ITransaction> FindByTransId(string transId);

        Task UpdateTransactionStatus(string transId, ActionStage status);

        Task RemoveTransaction(string transId);

        Task<int> RemoveTransactionByDate(DateTime date);

        Task CreateParticipant([NotNull] IParticipant participant);

        Task<IReadOnlyCollection<IParticipant>> FindParticipant(string transId, string participantId);

        Task<bool> ExistParticipantByTransId(string transId);

        Task UpdateParticipantStatus(string transId, string participantId, ActionStage status);

        Task RemoveParticipant(string transId, string participantId);

        Task<int> RemoveParticipantByDate(DateTime date);

        Task<bool> LockParticipant(IParticipant participant);

        Task<IReadOnlyCollection<IParticipant>> ListParticipant(DateTime dateTime, TransactionType transactionType,
            int limit);

        Task<IReadOnlyCollection<ITransaction>> ListLimitByDelay(DateTime dateTime, int limit);

        Task<IReadOnlyCollection<IParticipant>> ListParticipantByTransId(string transId);
    }
}