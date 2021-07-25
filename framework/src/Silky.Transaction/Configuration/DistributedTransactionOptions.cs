using Silky.Transaction.Abstraction;

namespace Silky.Transaction.Configuration
{
    public class DistributedTransactionOptions
    {
        public static string DistributedTransaction = "DistributedTransaction";

        public TransRepositorySupport UndoLogRepositorySupport { get; set; } = TransRepositorySupport.Redis;
    }
}