using System.Threading.Tasks;
using System.Transactions;
using Silky.Core;
using Silky.Core.Configuration;
using Silky.Transaction.Configuration;
using Silky.Transaction.Repository.Spi;

namespace Silky.Transaction.Repository
{
    public static class TransRepositoryStore
    {
        private static ITransRepository _transRepository;

        static TransRepositoryStore()
        {
            var appsettingOptions = EngineContext.Current.GetOptions<AppSettingsOptions>();
            var transactionOptions = EngineContext.Current.GetOptions<DistributedTransactionOptions>();
            _transRepository =
                EngineContext.Current.ResolveNamed<ITransRepository>(transactionOptions.RepositorySupport.ToString());

            if (_transRepository == null)
            {
                throw new TransactionException(
                    "Failed to obtain the distributed log storage repository, please set the distributed transaction log storage module");
            }
        }

        public static async Task SaveTransaction(ITransaction transaction)
        {
            await _transRepository.SaveTransaction(transaction);
        }
    }
}