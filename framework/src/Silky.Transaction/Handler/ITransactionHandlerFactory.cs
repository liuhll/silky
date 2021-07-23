using Silky.Rpc.Runtime.Server;
using Silky.Transaction.Repository.Spi;

namespace Silky.Transaction.Handler
{
    public interface ITransactionHandlerFactory
    {
        ITransactionHandler FactoryOf(TransactionContext context, ServiceEntry serviceEntry, string serviceKey);
    }
}