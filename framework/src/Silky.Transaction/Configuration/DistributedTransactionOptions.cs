using Silky.Lock;
using Silky.Transaction.Abstraction;

namespace Silky.Transaction.Configuration
{
    public class DistributedTransactionOptions
    {
        public static string DistributedTransaction = "DistributedTransaction";

        public TransactionType TransactionType { get; set; } = TransactionType.Tcc;

        public int ScheduledRecoveryDelay { get; set; } = 30;

        public int ScheduledCleanDelay { get; set; } = 120;

        public int ScheduledInitDelay { get; set; } = 10;

        public int RecoverDelayTime { get; set; } = 120;

        public int CleanDelayTime { get; set; } = 120;

        public int Limit { get; set; } = 100;

        public int RetryMax { get; set; } = 10;

        public bool PhyDeleted { get; set; } = true;

        public int StoreDays { get; set; } = 3;
    }
}