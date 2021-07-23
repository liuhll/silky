using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Silky.Transaction.Repository.Spi.Participant;

namespace Silky.Transaction.Repository.Spi
{
    public interface ITransaction
    {
        string TransId { get; }

        ActionStage Status { get; set; }

        TransactionType TransType { get; set; }

        DateTime CreateTime { get; }

        DateTime UpdateTime { get; set; }

        int ReTry { get; set; }
        
        [JsonIgnore]
        IReadOnlyCollection<IParticipant> Participants { get; }

        void RegisterParticipant(IParticipant participant);

        void RemoveParticipant(IParticipant participant);

        void RegisterParticipantList(IEnumerable<IParticipant> participants);
    }
}