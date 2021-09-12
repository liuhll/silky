using System;
using Silky.Transaction.Abstraction;

namespace Silky.Transaction.Tcc.Diagnostics
{
    public class TccTryingExecuteEventData
    {
        public TransactionContext TransactionContext { get; set; }
        public bool IsExecuteSuccess { get; set; }
        public string ParticipantId { get; set; }

        public string ServiceEntryId { get; set; }

        public string ServiceKey { get; set; }

        public DateTime? CreateTime { get; set; }

        public DateTime? UpdateTime { get; set; }
    }
}