using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Silky.Transaction.Abstraction.Participant;

namespace Silky.Transaction.Abstraction
{
    public interface ITransaction
    {
        string TransId { get; set; }

        string HostName { get; set; }

        ActionStage Status { get; set; }

        TransactionType TransType { get; set; }

        DateTime CreateTime { get; set; }

        DateTime UpdateTime { get; set; }

        int ReTry { get; set; }

        int Version { get; set; }

        [JsonIgnore] IReadOnlyCollection<IParticipant> Participants { get; }

        void RegisterParticipant(IParticipant participant);
    }
}