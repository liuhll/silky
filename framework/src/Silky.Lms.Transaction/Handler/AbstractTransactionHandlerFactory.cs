using System.Collections.Generic;
using Silky.Lms.Rpc.Runtime.Server;

namespace Silky.Lms.Transaction.Handler
{
    public abstract class AbstractTransactionHandlerFactory : ITransactionHandlerFactory
    {
        protected abstract IDictionary<TransactionRole, ITransactionHandler> Handlers { get; }
        
        public abstract ITransactionHandler FactoryOf(TransactionContext context, ServiceEntry serviceEntry, string serviceKey);

    }
}