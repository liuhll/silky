using Silky.Lms.Rpc.Runtime.Server;

namespace Silky.Lms.Transaction.Handler
{
    public interface ITransactionHandlerFactory
    {
        ITransactionHandler FactoryOf(TransactionContext context, ServiceEntry serviceEntry, string serviceKey);
    }
}