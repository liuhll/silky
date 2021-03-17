using System.Collections.Generic;
using Lms.Rpc.Runtime.Server;

namespace Lms.Transaction.Handler
{
    public abstract class AbstractTransactionHandlerFactory : ITransactionHandlerFactory
    {
        protected abstract IDictionary<TransactionRole, ITransactionHandler> Handlers { get; }
        
        public abstract ITransactionHandler FactoryOf(TransactionContext context, ServiceEntry serviceEntry, string serviceKey);

    }
}