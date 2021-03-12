using Lms.Core.DynamicProxy;
using Lms.Rpc.Runtime.Server;

namespace Lms.Rpc.Transaction
{
    public interface ITransactionHandlerFactory
    {
        ITransactionHandler FactoryOf(TransactionContext context, ServiceEntry serviceEntry, string serviceKey);
    }
}