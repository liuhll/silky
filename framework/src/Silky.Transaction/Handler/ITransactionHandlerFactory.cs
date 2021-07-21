using Silky.Rpc.Runtime.Server;

namespace Silky.Transaction.Handler
{
    public interface ITransactionHandlerFactory
    {
        ITransactionHandler FactoryOf(TransactionContext context, ServiceEntry serviceEntry, string serviceKey);
    }
}