namespace Silky.Transaction.Abstraction.Diagnostics
{
    public class GlobalTransactionEventData
    {
        public ITransaction Transaction { get; set; }

        public string ServiceEntryId { get; set; }

        public TransactionRole Role { get; set; }

        public TransactionType Type { get; set; }
    }
}