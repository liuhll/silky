namespace Silky.Transaction
{
    public class TransactionContext
    {
        public string TransId { get; set; }

        public string ParticipantId { get; set; }

        public ActionStage Action { get; set; }

        public TransactionRole TransactionRole { get; set; }

        public TransactionType TransType { get; set; }
    }
}