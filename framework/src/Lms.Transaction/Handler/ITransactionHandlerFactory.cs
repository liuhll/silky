using Lms.Rpc.Runtime.Server;

namespace Lms.Transaction.Handler
{
    public interface ITransactionHandlerFactory
    {
        ITransactionHandler FactoryOf(TransactionContext context, ServiceEntry serviceEntry, string serviceKey);
    }
}