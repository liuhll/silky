namespace Lms.Rpc.Transaction
{
    public class TransactionContext
    {
        public string TransId { get; set; }

        public string ParticipantId { get; set; }
        
        public TransactionRole TransactionRole { get; set; }
    }
}