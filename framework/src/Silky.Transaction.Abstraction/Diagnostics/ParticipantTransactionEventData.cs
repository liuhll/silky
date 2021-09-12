namespace Silky.Transaction.Abstraction.Diagnostics
{
    public class ParticipantTransactionEventData
    {
        public TransactionContext Context { get; set; }
        public TransactionRole Role { get; set; }

        public TransactionType Type { get; set; }
    }
}