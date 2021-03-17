using System;
using System.Collections.Generic;
using Lms.Transaction.Participant;

namespace Lms.Transaction
{
    public interface ITransaction
    {
        string TransId { get; }

        ActionStage Status { get; set; }

        TransactionType TransType { get; set; }

        DateTime CreateTime { get; }

        DateTime UpdateTime { get; set; }

        int ReTry { get; set; }

        IReadOnlyCollection<IParticipant> Participants { get; }

        void RegisterParticipant(IParticipant participant);

        void RemoveParticipant(IParticipant participant);

        void RegisterParticipantList(IEnumerable<IParticipant> participants);
    }
}