using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Silky.Transaction.Repository.Spi.Participant;

namespace Silky.Transaction.Repository.Spi
{
    public interface ITransRepository
    {
        Task SaveTransaction([NotNull] ITransaction transaction);

        Task<ITransaction> FindByTransId(string transId);

        Task UpdateTransactionStatus(string transId, ActionStage status);

        Task RemoveTransaction(string transId);

        Task<int> RemoveTransactionByDate(DateTime date);

        Task SaveParticipant([NotNull] IParticipant participant);

        Task<IParticipant> FindParticipant(string transId, string participantId);

        Task<IEnumerable<IParticipant>> ListParticipantByTransId(string transId);

        Task<bool> ExistParticipantByTransId(string transId);

        Task UpdateParticipantStatus(string transId, string participantId, ActionStage status);

        Task RemoveParticipant(string transId, string participantId);

        Task<int> RemoveParticipantByDate(DateTime date);

        Task<bool> LockParticipant(IParticipant participant);


        // Task<int> CreateParticipantUndo(IParticipantUndo participantUndo);
        //
        // Task<IEnumerable<HmilyParticipantUndo>> FindParticipantUndoByParticipantId(string participantId);
        //
        //
        // Task<int> RemoveParticipantUndo(string undoId);
        //
        //
        // Task<int> RemoveParticipantUndoByDate(DateTime date);
        //
        // Task<int> UpdateHmilyParticipantUndoStatus(string undoId, ActionStage status);
    }
}