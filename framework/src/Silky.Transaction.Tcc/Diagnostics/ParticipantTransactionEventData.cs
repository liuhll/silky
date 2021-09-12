using System;
using Silky.Rpc.Runtime.Server;
using Silky.Transaction.Abstraction;

namespace Silky.Transaction.Tcc.Diagnostics
{
    public class ParticipantTransactionEventData
    {
        public ServiceEntry ServiceEntry { get; set; }

        public ActionStage Status { get; set; }

        public TransactionType TransType { get; set; }

        public string TransId { get; set; }

        public string HostName { get; set; }

        public int ReTry { get; set; }

        public int Version { get; set; }
        public string ParticipantId { get; set; }

        public string ServiceEntryId { get; set; }

        public string ServiceKey { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime UpdateTime { get; set; }
    }
}