using System;
using System.Collections.Generic;
using Silky.Transaction.Abstraction;

namespace Silky.Transaction.Tcc.Diagnostics
{
    public class GlobalTransactionEventData
    {
        public string TransId { get; set; }

        public string Method { get; set; }

        public string HostName { get; set; }

        public ActionStage Status { get; set; }

        public TransactionType TransType { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime UpdateTime { get; set; }

        public int ReTry { get; set; }

        public int Version { get; set; }
        
        public string ServiceEntryId { get; set; }

        public IReadOnlyCollection<ParticipantTransactionEventData> Participants { get; set; }
        
    }
}