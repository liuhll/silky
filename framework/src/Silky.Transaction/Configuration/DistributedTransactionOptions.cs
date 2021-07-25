using Silky.Transaction.Repository.Spi;

namespace Silky.Transaction.Configuration
{
    public class DistributedTransactionOptions
    {
        public static string DistributedTransaction = "DistributedTransaction";

        public TransRepositorySupport UndoLogRepositorySupport { get; set; } = TransRepositorySupport.Redis;
    }
}