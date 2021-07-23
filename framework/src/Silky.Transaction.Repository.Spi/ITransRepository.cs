using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Silky.Transaction.Repository.Spi.Participant;

namespace Silky.Transaction.Repository.Spi
{
    public interface ITransRepository
    {
        Task SaveTransaction(ITransaction transaction);

        Task<ITransaction> FindByTransId(string transId);

        Task UpdateTransactionStatus(string transId, ActionStage status);

        Task RemoveTransaction(string transId);
        
        Task<int> RemoveTransactionByDate(DateTime date);
        
        Task<bool> CreateParticipant(IParticipant participant);

        Task<IParticipant> FindParticipant(string participantId);
        
        Task<IEnumerable<IParticipant>> ListParticipant(DateTime date, TransactionType transType, int limit);

        Task<IEnumerable<IParticipant>> ListParticipantByTransId(string transId);
        
        Task<bool> ExistParticipantByTransId(string transId);
        
        Task<int> UpdateParticipantStatus(string participantId, ActionStage status);
        
        Task<bool> RemoveParticipant(string participantId);
        
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